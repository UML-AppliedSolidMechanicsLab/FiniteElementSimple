/*
 * Basic automated test for the VTK XML UnstructuredGrid (.vtu) exporter.
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using FiniteElementSimple;
using FiniteElementSimple.Elements;
using FiniteElementSimple.Materials;
using FiniteElementSimple.BoundaryConditions;
using FiniteElementSimple.Output;

namespace FiniteElementSimple.Tests
{
    [TestFixture]
    public class VtuWriterTests
    {
        [Test]
        public void TestWriteSolvedQ8Model()
        {
            //Build a simple single-Q8-element solved assembly (same setup style as
            //Quadratic8NodeTests.TestKMatrix_EricsExample).
            LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(43, 0.3);
            double thickness = 1.3;

            int[][] ConnectivityMatrix = { new int[] { 1, 2, 3, 4, 5, 6, 7, 8 } };

            double[][] NodalLocations = new double[][]{
                new double[]{0,0},
                new double[]{2,0.3},
                new double[]{1.9,3.4},
                new double[]{-0.1,3},
                new double[]{0.7,0},
                new double[]{1.8,1.9},
                new double[]{1.1,3.2},
                new double[]{0,3},
            };

            Node8Element2D myQuad = new Node8Element2D(myMaterial, ConnectivityMatrix[0], thickness, NodalLocations);

            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            //Fix node 1 in both directions, and node 2 in the y direction, to make the model solvable.
            lBCs.Add(new BC(1, 0, 2, 0.0));
            lBCs.Add(new BC(1, 1, 2, 0.0));
            lBCs.Add(new BC(2, 1, 2, 0.0));
            //Apply a small load at node 3 in the x direction.
            lLoads.Add(new BC(3, 0, 2, 100.0));

            List<Element> lElements = new List<Element> { myQuad };

            Assembly myAssembly = new Assembly(lElements, lLoads, lBCs, 2);
            myAssembly.Solve();

            string filePath = Path.Combine(Path.GetTempPath(), "VtuWriterTest_" + Guid.NewGuid().ToString("N") + ".vtu");
            try
            {
                VtuWriter.Write(myAssembly, filePath);

                //File was created
                Assert.IsTrue(File.Exists(filePath));

                string content = File.ReadAllText(filePath);

                //Reported number of points and cells is correct (single Q8 element -> 8 unique points, 1 cell)
                Assert.IsTrue(content.Contains("NumberOfPoints=\"8\""));
                Assert.IsTrue(content.Contains("NumberOfCells=\"1\""));

                //Expected VTK cell type (VTK_QUADRATIC_QUAD = 23) is present
                Assert.IsTrue(content.Contains("Name=\"types\""));
                Assert.IsTrue(content.Contains("          23"));

                //Displacement data is present
                Assert.IsTrue(content.Contains("Name=\"Displacement\""));

                //Connectivity uses valid zero-based node indices (0..7 for 8 points, no duplicates other than reuse across cells)
                string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string connectivityLine = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("Name=\"connectivity\""))
                    {
                        connectivityLine = lines[i + 1].Trim();
                        break;
                    }
                }
                Assert.IsNotNull(connectivityLine, "Could not find connectivity data in .vtu file");

                int[] connectivityIndices = connectivityLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(int.Parse).ToArray();
                Assert.AreEqual(8, connectivityIndices.Length);
                foreach (int index in connectivityIndices)
                {
                    Assert.GreaterOrEqual(index, 0);
                    Assert.Less(index, 8);
                }
                Assert.AreEqual(Enumerable.Range(0, 8).OrderBy(x => x), connectivityIndices.OrderBy(x => x));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static Assembly BuildSolvedSingleQ8Assembly(out Node8Element2D myQuad)
        {
            LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(43, 0.3);
            double thickness = 1.3;

            int[] connectivity = { 1, 2, 3, 4, 5, 6, 7, 8 };

            double[][] NodalLocations = new double[][]{
                new double[]{0,0},
                new double[]{2,0.3},
                new double[]{1.9,3.4},
                new double[]{-0.1,3},
                new double[]{0.7,0},
                new double[]{1.8,1.9},
                new double[]{1.1,3.2},
                new double[]{0,3},
            };

            myQuad = new Node8Element2D(myMaterial, connectivity, thickness, NodalLocations);

            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            lBCs.Add(new BC(1, 0, 2, 0.0));
            lBCs.Add(new BC(1, 1, 2, 0.0));
            lBCs.Add(new BC(2, 1, 2, 0.0));
            lLoads.Add(new BC(3, 0, 2, 100.0));

            List<Element> lElements = new List<Element> { myQuad };

            Assembly myAssembly = new Assembly(lElements, lLoads, lBCs, 2);
            myAssembly.Solve();
            return myAssembly;
        }

        [Test]
        public void TestWriteSampledVisualization_DefaultResolution()
        {
            Node8Element2D myQuad;
            Assembly myAssembly = BuildSolvedSingleQ8Assembly(out myQuad);

            string filePath = Path.Combine(Path.GetTempPath(), "VtuWriterTest_Sampled_" + Guid.NewGuid().ToString("N") + ".vtu");
            try
            {
                VtuWriter.WriteSampledVisualization(myAssembly, filePath);

                Assert.IsTrue(File.Exists(filePath));
                string content = File.ReadAllText(filePath);

                //Default resolution is 5x5 samples per element -> 25 points and 4x4=16 VTK_QUAD sub-cells.
                Assert.IsTrue(content.Contains("NumberOfPoints=\"25\""));
                Assert.IsTrue(content.Contains("NumberOfCells=\"16\""));

                Assert.IsTrue(content.Contains("Name=\"Displacement\""));
                Assert.IsTrue(content.Contains("Name=\"Strain\""));
                Assert.IsTrue(content.Contains("Name=\"Stress\""));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Test]
        public void TestWriteSampledVisualization_ConfigurableResolution()
        {
            Node8Element2D myQuad;
            Assembly myAssembly = BuildSolvedSingleQ8Assembly(out myQuad);

            string filePath = Path.Combine(Path.GetTempPath(), "VtuWriterTest_Sampled3x4_" + Guid.NewGuid().ToString("N") + ".vtu");
            try
            {
                VtuWriter.WriteSampledVisualization(myAssembly, filePath, nXi: 3, nEta: 4);

                Assert.IsTrue(File.Exists(filePath));
                string content = File.ReadAllText(filePath);

                //3x4 grid -> 12 points, 2x3=6 VTK_QUAD sub-cells.
                Assert.IsTrue(content.Contains("NumberOfPoints=\"12\""));
                Assert.IsTrue(content.Contains("NumberOfCells=\"6\""));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Test]
        public void TestWriteIntegrationPointResults()
        {
            Node8Element2D myQuad;
            Assembly myAssembly = BuildSolvedSingleQ8Assembly(out myQuad);

            int expectedIntegrationPoints = myQuad.GaussPoints.Count();

            string filePath = Path.Combine(Path.GetTempPath(), "VtuWriterTest_IntPts_" + Guid.NewGuid().ToString("N") + ".vtu");
            try
            {
                VtuWriter.WriteIntegrationPointResults(myAssembly, filePath);

                Assert.IsTrue(File.Exists(filePath));
                string content = File.ReadAllText(filePath);

                Assert.IsTrue(content.Contains("NumberOfPoints=\"" + expectedIntegrationPoints + "\""));
                Assert.IsTrue(content.Contains("NumberOfCells=\"" + expectedIntegrationPoints + "\""));

                //VTK_VERTEX = 1
                Assert.IsTrue(content.Contains("Name=\"types\""));
                Assert.IsTrue(content.Contains("Name=\"Strain\""));
                Assert.IsTrue(content.Contains("Name=\"Stress\""));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
