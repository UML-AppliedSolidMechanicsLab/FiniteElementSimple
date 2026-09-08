/*
 * Basic ParaView (.vtu) export.
 * Writes a solved 2-D finite-element Assembly (Node4Element2D / Node8Element2D) to ASCII
 * VTK XML UnstructuredGrid (.vtu) files so results can be opened directly in ParaView.
 *
 * This class distinguishes between three different kinds of output, which must not be confused:
 *  - FE nodes/elements (Write):
 *      The actual finite-element mesh - one point per unique global FE node, one cell per element,
 *      with nodal Displacement as the only point data.
 *  - Integration-point results (WriteIntegrationPointResults):
 *      One VTK_VERTEX point per actual Gaussian integration point used by the FE solution, with the
 *      Strain/Stress evaluated (and stored) at exactly that point. No extrapolation/averaging.
 *  - Sampled visualization points (WriteSampledVisualization):
 *      An independent, per-element n_xi x n_eta grid of points in natural coordinates, purely for
 *      smoother visualization of Displacement/Strain/Stress fields. These points are unrelated to the
 *      FE nodes or the integration points, and are never shared/averaged across elements.
 *
 * Supported element types (2-D only, in this implementation):
 *  - Q4 elements  (Node4Element2D) -> VTK_QUAD (9)
 *  - Q8 elements  (Node8Element2D) -> VTK_QUADRATIC_QUAD (23)
 * Sampled-visualization and integration-point cells are always emitted as VTK_QUAD (9) sub-cells or
 * VTK_VERTEX (1) points respectively, regardless of the parent element type.
 *
 * Assembly itself has no knowledge of this class; VtuWriter only reads public data from Assembly/Element.
 * The FE solution itself (K/F/BC handling/solver) is never modified by anything in this class.
 */
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple.Output
{
    /// <summary>
    /// Writes a solved 2-D Assembly (Q4/Q8 elements) to VTK XML UnstructuredGrid (.vtu) files, in one
    /// of three modes: the raw FE mesh, sampled visualization points, or integration-point results.
    /// </summary>
    public static class VtuWriter
    {
        private const int VTK_VERTEX = 1;
        private const int VTK_QUAD = 9;
        private const int VTK_QUADRATIC_QUAD = 23;

        /// <summary>
        /// Default number of sampled visualization points per element, per natural-coordinate direction.
        /// </summary>
        public const int DefaultSamplesPerDirection = 5;

        //Local-node parametric (xi, eta) locations, in the order VTK expects for each cell type.
        //These also match the local node ordering already used by Node4Element2D/Node8Element2D's
        //shape functions (corners counter-clockwise, then mid-edge nodes for the Q8), so no
        //connectivity reordering is required.
        private static readonly double[][] Q4ParametricNodes = new double[][]
        {
            new double[] {-1, -1}, new double[] {1, -1}, new double[] {1, 1}, new double[] {-1, 1}
        };

        private static readonly double[][] Q8ParametricNodes = new double[][]
        {
            new double[] {-1, -1}, new double[] {1, -1}, new double[] {1, 1}, new double[] {-1, 1},
            new double[] {0, -1}, new double[] {1, 0}, new double[] {0, 1}, new double[] {-1, 0}
        };

        /// <summary>
        /// Writes the given (solved) Assembly to an ASCII .vtu file at filePath.
        /// Only Node4Element2D and Node8Element2D elements are exported; other element types are skipped.
        /// </summary>
        public static void Write(Assembly assembly, string filePath)
        {
            Dictionary<int, int> pointIndexByGlobalNodeNumber = new Dictionary<int, int>();
            List<double[]> pointCoordinates = new List<double[]>();
            List<double[]> pointDisplacements = new List<double[]>();

            List<int[]> cellConnectivity = new List<int[]>();
            List<int> cellTypes = new List<int>();

            foreach (Element element in assembly.lElements)
            {
                double[][] parametricNodes;
                int vtkCellType;

                if (element is Node4Element2D)
                {
                    parametricNodes = Q4ParametricNodes;
                    vtkCellType = VTK_QUAD;
                }
                else if (element is Node8Element2D)
                {
                    parametricNodes = Q8ParametricNodes;
                    vtkCellType = VTK_QUADRATIC_QUAD;
                }
                else
                {
                    //Only 2-D Q4/Q8 elements are supported in this first implementation.
                    continue;
                }

                int[] connectivity = new int[parametricNodes.Length];

                for (int localNodeIndex = 0; localNodeIndex < parametricNodes.Length; localNodeIndex++)
                {
                    int globalNodeNumber = element.localToGlobalConnectivity[localNodeIndex];

                    int pointIndex;
                    if (!pointIndexByGlobalNodeNumber.TryGetValue(globalNodeNumber, out pointIndex))
                    {
                        double xi = parametricNodes[localNodeIndex][0];
                        double eta = parametricNodes[localNodeIndex][1];

                        double[] globalPosition = element.GlobalXPosition(xi, eta, 0.0);

                        double ux = element.Q[localNodeIndex * element.nDOFperNode + 0];
                        double uy = element.nDOFperNode > 1 ? element.Q[localNodeIndex * element.nDOFperNode + 1] : 0.0;

                        pointIndex = pointCoordinates.Count;
                        pointCoordinates.Add(new double[] { globalPosition[0], globalPosition[1], 0.0 });
                        pointDisplacements.Add(new double[] { ux, uy, 0.0 });
                        pointIndexByGlobalNodeNumber[globalNodeNumber] = pointIndex;
                    }

                    connectivity[localNodeIndex] = pointIndex;
                }

                cellConnectivity.Add(connectivity);
                cellTypes.Add(vtkCellType);
            }

            List<NamedPointData> pointDataArrays = new List<NamedPointData>
            {
                new NamedPointData("Displacement", 3, pointDisplacements)
            };

            WriteVtuFile(filePath, pointCoordinates, cellConnectivity, cellTypes, pointDataArrays);
        }

        /// <summary>
        /// Samples each 2-D element (Q4/Q8) on an independent nXi x nEta grid in natural coordinates
        /// (default approximately 5x5) and writes physical position, Displacement, Strain, and Stress
        /// at each sample point to filePath as a subdivided visualization mesh of VTK_QUAD sub-cells.
        /// These sampled points are separate from, and are not averaged/merged with, either the FE nodes
        /// or the integration points; no extrapolation to FE nodes and no inter-element averaging is done.
        /// </summary>
        public static void WriteSampledVisualization(Assembly assembly, string filePath,
            int nXi = DefaultSamplesPerDirection, int nEta = DefaultSamplesPerDirection)
        {
            if (nXi < 2) nXi = 2;
            if (nEta < 2) nEta = 2;

            List<double[]> pointCoordinates = new List<double[]>();
            List<double[]> displacementValues = new List<double[]>();
            List<double[]> strainValues = new List<double[]>();
            List<double[]> stressValues = new List<double[]>();

            List<int[]> cellConnectivity = new List<int[]>();
            List<int> cellTypes = new List<int>();

            foreach (Element element in assembly.lElements)
            {
                if (!(element is Node4Element2D) && !(element is Node8Element2D))
                {
                    //Only 2-D Q4/Q8 elements support sampled visualization in this implementation.
                    continue;
                }

                //Build an independent nXi x nEta grid of sample points for this element only; these
                //points are never shared or averaged with points sampled from any other element.
                int[,] gridPointIndex = new int[nXi, nEta];

                for (int i = 0; i < nXi; i++)
                {
                    double xi = -1.0 + 2.0 * i / (nXi - 1);
                    for (int j = 0; j < nEta; j++)
                    {
                        double eta = -1.0 + 2.0 * j / (nEta - 1);

                        //Map (xi, eta) to physical coordinates using the element's own interpolation,
                        //and evaluate displacement/strain/stress using the existing element/material
                        //formulations exactly as used for the FE solution (no changes to those formulas).
                        double[] globalPosition = element.GlobalXPosition(xi, eta, 0.0);
                        double[] displacement = element.Displacement(xi, eta, 0.0);
                        double[] strain = element.Strain(xi, eta, 0.0);
                        double[] stress = element.Stress(xi, eta, 0.0);

                        int pointIndex = pointCoordinates.Count;
                        pointCoordinates.Add(new double[] { globalPosition[0], globalPosition[1], 0.0 });
                        displacementValues.Add(new double[] { displacement[0], displacement.Length > 1 ? displacement[1] : 0.0, 0.0 });
                        strainValues.Add(ToThreeComponents(strain));
                        stressValues.Add(ToThreeComponents(stress));

                        gridPointIndex[i, j] = pointIndex;
                    }
                }

                //Connect the sample grid into small VTK_QUAD sub-cells to form a subdivided
                //visualization mesh (purely for rendering; not part of the FE mesh itself).
                for (int i = 0; i < nXi - 1; i++)
                {
                    for (int j = 0; j < nEta - 1; j++)
                    {
                        int p00 = gridPointIndex[i, j];
                        int p10 = gridPointIndex[i + 1, j];
                        int p11 = gridPointIndex[i + 1, j + 1];
                        int p01 = gridPointIndex[i, j + 1];

                        cellConnectivity.Add(new int[] { p00, p10, p11, p01 });
                        cellTypes.Add(VTK_QUAD);
                    }
                }
            }

            List<NamedPointData> pointDataArrays = new List<NamedPointData>
            {
                new NamedPointData("Displacement", 3, displacementValues),
                new NamedPointData("Strain", 3, strainValues),
                new NamedPointData("Stress", 3, stressValues)
            };

            WriteVtuFile(filePath, pointCoordinates, cellConnectivity, cellTypes, pointDataArrays);
        }

        /// <summary>
        /// Exports the actual Gaussian integration-point locations used by each 2-D element's FE
        /// integration (QuadraticElement2D.GaussPoints), along with the Strain/Stress evaluated at
        /// exactly those points, as VTK_VERTEX points. No extrapolation to FE nodes and no averaging
        /// between neighboring elements or integration points is performed.
        /// </summary>
        public static void WriteIntegrationPointResults(Assembly assembly, string filePath)
        {
            List<double[]> pointCoordinates = new List<double[]>();
            List<double[]> strainValues = new List<double[]>();
            List<double[]> stressValues = new List<double[]>();

            List<int[]> cellConnectivity = new List<int[]>();
            List<int> cellTypes = new List<int>();

            foreach (Element element in assembly.lElements)
            {
                QuadraticElement2D quadraticElement = element as QuadraticElement2D;
                if (quadraticElement == null)
                {
                    //Integration-point export currently only supports 2-D Q4/Q8 elements.
                    continue;
                }

                foreach ((double xi, double eta) in quadraticElement.GaussPoints)
                {
                    double[] globalPosition = quadraticElement.GlobalXPosition(xi, eta, 0.0);
                    double[] strain = quadraticElement.Strain(xi, eta, 0.0);
                    double[] stress = quadraticElement.Stress(xi, eta, 0.0);

                    int pointIndex = pointCoordinates.Count;
                    pointCoordinates.Add(new double[] { globalPosition[0], globalPosition[1], 0.0 });
                    strainValues.Add(ToThreeComponents(strain));
                    stressValues.Add(ToThreeComponents(stress));

                    cellConnectivity.Add(new int[] { pointIndex });
                    cellTypes.Add(VTK_VERTEX);
                }
            }

            List<NamedPointData> pointDataArrays = new List<NamedPointData>
            {
                new NamedPointData("Strain", 3, strainValues),
                new NamedPointData("Stress", 3, stressValues)
            };

            WriteVtuFile(filePath, pointCoordinates, cellConnectivity, cellTypes, pointDataArrays);
        }

        private static double[] ToThreeComponents(double[] values)
        {
            double[] result = new double[3];
            for (int i = 0; i < values.Length && i < 3; i++)
            {
                result[i] = values[i];
            }
            return result;
        }

        /// <summary>
        /// A single named point-data array (e.g. Displacement, Strain, Stress) to be written to a .vtu file.
        /// </summary>
        private readonly struct NamedPointData
        {
            public readonly string Name;
            public readonly int NumberOfComponents;
            public readonly List<double[]> Values;

            public NamedPointData(string name, int numberOfComponents, List<double[]> values)
            {
                Name = name;
                NumberOfComponents = numberOfComponents;
                Values = values;
            }
        }

        private static void WriteVtuFile(string filePath, List<double[]> pointCoordinates,
            List<int[]> cellConnectivity, List<int> cellTypes, List<NamedPointData> pointDataArrays)
        {
            int nPoints = pointCoordinates.Count;
            int nCells = cellConnectivity.Count;

            int[] offsets = new int[nCells];
            int runningOffset = 0;
            for (int i = 0; i < nCells; i++)
            {
                runningOffset += cellConnectivity[i].Length;
                offsets[i] = runningOffset;
            }

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>");
                writer.WriteLine("<VTKFile type=\"UnstructuredGrid\" version=\"0.1\" byte_order=\"LittleEndian\">");
                writer.WriteLine("  <UnstructuredGrid>");
                writer.WriteLine("    <Piece NumberOfPoints=\"" + nPoints + "\" NumberOfCells=\"" + nCells + "\">");

                writer.WriteLine("      <Points>");
                writer.WriteLine("        <DataArray type=\"Float64\" NumberOfComponents=\"3\" format=\"ascii\">");
                foreach (double[] p in pointCoordinates)
                {
                    writer.WriteLine("          " + FormatDouble(p[0]) + " " + FormatDouble(p[1]) + " " + FormatDouble(p[2]));
                }
                writer.WriteLine("        </DataArray>");
                writer.WriteLine("      </Points>");

                writer.WriteLine("      <PointData" + (pointDataArrays.Count > 0 ? " Vectors=\"" + pointDataArrays[0].Name + "\"" : "") + ">");
                foreach (NamedPointData pointData in pointDataArrays)
                {
                    writer.WriteLine("        <DataArray type=\"Float64\" Name=\"" + pointData.Name + "\" NumberOfComponents=\"" + pointData.NumberOfComponents + "\" format=\"ascii\">");
                    foreach (double[] v in pointData.Values)
                    {
                        writer.WriteLine("          " + string.Join(" ", System.Linq.Enumerable.Range(0, pointData.NumberOfComponents).Select(k => FormatDouble(v[k]))));
                    }
                    writer.WriteLine("        </DataArray>");
                }
                writer.WriteLine("      </PointData>");

                writer.WriteLine("      <Cells>");
                writer.WriteLine("        <DataArray type=\"Int32\" Name=\"connectivity\" format=\"ascii\">");
                foreach (int[] connectivity in cellConnectivity)
                {
                    writer.WriteLine("          " + string.Join(" ", connectivity));
                }
                writer.WriteLine("        </DataArray>");

                writer.WriteLine("        <DataArray type=\"Int32\" Name=\"offsets\" format=\"ascii\">");
                writer.WriteLine("          " + string.Join(" ", offsets));
                writer.WriteLine("        </DataArray>");

                writer.WriteLine("        <DataArray type=\"UInt8\" Name=\"types\" format=\"ascii\">");
                writer.WriteLine("          " + string.Join(" ", cellTypes));
                writer.WriteLine("        </DataArray>");
                writer.WriteLine("      </Cells>");

                writer.WriteLine("    </Piece>");
                writer.WriteLine("  </UnstructuredGrid>");
                writer.WriteLine("</VTKFile>");
            }
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
