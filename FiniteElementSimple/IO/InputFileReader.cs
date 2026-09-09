/*
 * Simple text-based finite-element input-file reader.
 *
 * Scope (first implementation): existing linear 2-D problems only (Q4/Q8 plane-stress elements).
 * The reader does not implement any FE mechanics itself - it only parses a readable
 * keyword/section text format and constructs the same Assembly/Element/Material/BC objects
 * that hard-coded example problems already construct directly in code.
 *
 * File format:
 *
 *   *MATERIALS
 *   materialId, PLANESTRESS, E, nu
 *
 *   *NODES
 *   nodeId, x, y
 *
 *   *THICKNESS
 *   thickness
 *
 *   *ELEMENTS
 *   elementId, Q4|Q8, materialId, node1, node2, ... (4 or 8 node ids)
 *
 *   *BCS
 *   nodeId, dofInNode, magnitude
 *
 *   *LOADS
 *   nodeId, dofInNode, magnitude
 *
 * Rules:
 *  - Sections start with a line beginning with '*' followed by the section keyword.
 *  - Data lines within a section are comma-separated fields.
 *  - Blank lines and lines starting with '#' are treated as comments and ignored everywhere.
 *  - All problems produced by this reader use nDOFperNode = 2 (2-D only, per scope).
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FiniteElementSimple.Elements;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.IO
{
    /// <summary>
    /// Reads a simple text-based finite-element input file and constructs the corresponding
    /// Assembly (materials, nodes, Q4/Q8 elements, displacement BCs, and nodal loads).
    /// </summary>
    public static class InputFileReader
    {
        private const int nDOFperNode = 2;

        public static Assembly Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("FE input file not found: " + filePath, filePath);
            }

            string[] lines = File.ReadAllLines(filePath);

            Dictionary<int, Material> materialsById = new Dictionary<int, Material>();
            Dictionary<int, double[]> nodeLocationsById = new Dictionary<int, double[]>();
            double? thickness = null;
            List<ElementRecord> elementRecords = new List<ElementRecord>();
            List<BC> lBCs = new List<BC>();
            List<BC> lLoads = new List<BC>();

            string currentSection = null;

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                int lineNumber = lineIndex + 1;
                string rawLine = lines[lineIndex];
                string line = rawLine.Trim();

                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("*"))
                {
                    currentSection = line.Substring(1).Trim().ToUpperInvariant();

                    switch (currentSection)
                    {
                        case "MATERIALS":
                        case "NODES":
                        case "THICKNESS":
                        case "ELEMENTS":
                        case "BCS":
                        case "LOADS":
                            break;
                        default:
                            throw new FormatException(
                                "Line " + lineNumber + ": unknown section keyword \"*" + currentSection + "\". " +
                                "Supported sections are *MATERIALS, *NODES, *THICKNESS, *ELEMENTS, *BCS, *LOADS.");
                    }

                    continue;
                }

                if (currentSection == null)
                {
                    throw new FormatException(
                        "Line " + lineNumber + ": data found before any section header (expected a line starting with '*', e.g. *NODES).");
                }

                string[] fields = line.Split(',').Select(f => f.Trim()).ToArray();

                switch (currentSection)
                {
                    case "MATERIALS":
                        ParseMaterialLine(fields, lineNumber, materialsById);
                        break;
                    case "NODES":
                        ParseNodeLine(fields, lineNumber, nodeLocationsById);
                        break;
                    case "THICKNESS":
                        thickness = ParseThicknessLine(fields, lineNumber, thickness);
                        break;
                    case "ELEMENTS":
                        elementRecords.Add(ParseElementLine(fields, lineNumber));
                        break;
                    case "BCS":
                        lBCs.Add(ParseBcOrLoadLine(fields, lineNumber, nodeLocationsById, "*BCS"));
                        break;
                    case "LOADS":
                        lLoads.Add(ParseBcOrLoadLine(fields, lineNumber, nodeLocationsById, "*LOADS"));
                        break;
                }
            }

            if (!thickness.HasValue)
            {
                throw new FormatException("Missing required *THICKNESS section (a single element thickness value).");
            }

            if (elementRecords.Count == 0)
            {
                throw new FormatException("Missing required *ELEMENTS section (no elements were defined).");
            }

            List<Element> lElements = new List<Element>();
            foreach (ElementRecord record in elementRecords)
            {
                lElements.Add(BuildElement(record, materialsById, nodeLocationsById, thickness.Value));
            }

            return new LinearAssembly(lElements, lLoads, lBCs, nDOFperNode);
        }

        private class ElementRecord
        {
            public int ElementId;
            public int LineNumber;
            public string ElementType;
            public int MaterialId;
            public int[] NodeIds;
        }

        private static void ParseMaterialLine(string[] fields, int lineNumber, Dictionary<int, Material> materialsById)
        {
            //materialId, PLANESTRESS, E, nu
            if (fields.Length != 4)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": malformed *MATERIALS line \"" + string.Join(",", fields) + "\". " +
                    "Expected format: materialId, PLANESTRESS, E, nu");
            }

            int materialId = ParseInt(fields[0], lineNumber, "materialId");
            string materialType = fields[1].ToUpperInvariant();

            if (materialsById.ContainsKey(materialId))
            {
                throw new FormatException("Line " + lineNumber + ": duplicate material id " + materialId + ".");
            }

            Material material;
            switch (materialType)
            {
                case "PLANESTRESS":
                    double E = ParseDouble(fields[2], lineNumber, "E");
                    double nu = ParseDouble(fields[3], lineNumber, "nu");
                    material = new LinElastic2DPlaneStress(E, nu);
                    break;
                default:
                    throw new FormatException(
                        "Line " + lineNumber + ": unsupported material type \"" + fields[1] + "\". Only PLANESTRESS is supported.");
            }

            materialsById[materialId] = material;
        }

        private static void ParseNodeLine(string[] fields, int lineNumber, Dictionary<int, double[]> nodeLocationsById)
        {
            //nodeId, x, y
            if (fields.Length != 3)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": malformed *NODES line \"" + string.Join(",", fields) + "\". " +
                    "Expected format: nodeId, x, y");
            }

            int nodeId = ParseInt(fields[0], lineNumber, "nodeId");
            double x = ParseDouble(fields[1], lineNumber, "x");
            double y = ParseDouble(fields[2], lineNumber, "y");

            if (nodeLocationsById.ContainsKey(nodeId))
            {
                throw new FormatException("Line " + lineNumber + ": duplicate node number " + nodeId + ".");
            }

            nodeLocationsById[nodeId] = new double[] { x, y };
        }

        private static double ParseThicknessLine(string[] fields, int lineNumber, double? existingThickness)
        {
            //thickness
            if (fields.Length != 1)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": malformed *THICKNESS line \"" + string.Join(",", fields) + "\". " +
                    "Expected a single value: thickness");
            }

            if (existingThickness.HasValue)
            {
                throw new FormatException("Line " + lineNumber + ": *THICKNESS may only be specified once in this simple format.");
            }

            return ParseDouble(fields[0], lineNumber, "thickness");
        }

        private static ElementRecord ParseElementLine(string[] fields, int lineNumber)
        {
            //elementId, Q4|Q8, materialId, node1, node2, ...
            if (fields.Length < 4)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": malformed *ELEMENTS line \"" + string.Join(",", fields) + "\". " +
                    "Expected format: elementId, Q4|Q8, materialId, node1, node2, ...");
            }

            int elementId = ParseInt(fields[0], lineNumber, "elementId");
            string elementType = fields[1].ToUpperInvariant();
            int materialId = ParseInt(fields[2], lineNumber, "materialId");

            int expectedNodeCount;
            switch (elementType)
            {
                case "Q4":
                    expectedNodeCount = 4;
                    break;
                case "Q8":
                    expectedNodeCount = 8;
                    break;
                default:
                    throw new FormatException(
                        "Line " + lineNumber + ": unsupported element type \"" + fields[1] + "\". Only Q4 and Q8 are supported.");
            }

            int[] nodeIds = fields.Skip(3).Select(f => ParseInt(f, lineNumber, "node id")).ToArray();
            if (nodeIds.Length != expectedNodeCount)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": element " + elementId + " of type " + elementType + " requires exactly " +
                    expectedNodeCount + " node ids, but " + nodeIds.Length + " were given.");
            }

            return new ElementRecord
            {
                ElementId = elementId,
                LineNumber = lineNumber,
                ElementType = elementType,
                MaterialId = materialId,
                NodeIds = nodeIds
            };
        }

        private static BC ParseBcOrLoadLine(string[] fields, int lineNumber, Dictionary<int, double[]> nodeLocationsById, string sectionName)
        {
            //nodeId, dofInNode, magnitude
            if (fields.Length != 3)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": malformed " + sectionName + " line \"" + string.Join(",", fields) + "\". " +
                    "Expected format: nodeId, dofInNode, magnitude");
            }

            int nodeId = ParseInt(fields[0], lineNumber, "nodeId");
            int dofInNode = ParseInt(fields[1], lineNumber, "dofInNode");
            double magnitude = ParseDouble(fields[2], lineNumber, "magnitude");

            if (!nodeLocationsById.ContainsKey(nodeId))
            {
                throw new FormatException(
                    "Line " + lineNumber + ": " + sectionName + " references undefined node " + nodeId + ".");
            }

            if (dofInNode < 0 || dofInNode >= nDOFperNode)
            {
                throw new FormatException(
                    "Line " + lineNumber + ": " + sectionName + " dofInNode must be 0 or 1 for a 2-D problem, got " + dofInNode + ".");
            }

            return new BC(nodeId, dofInNode, nDOFperNode, magnitude);
        }

        private static Element BuildElement(ElementRecord record, Dictionary<int, Material> materialsById,
            Dictionary<int, double[]> nodeLocationsById, double thickness)
        {
            if (!materialsById.TryGetValue(record.MaterialId, out Material material))
            {
                throw new FormatException(
                    "Line " + record.LineNumber + ": element " + record.ElementId + " references undefined material id " + record.MaterialId + ".");
            }

            double[][] nodalLocations = new double[record.NodeIds.Length][];
            for (int i = 0; i < record.NodeIds.Length; i++)
            {
                int nodeId = record.NodeIds[i];
                if (!nodeLocationsById.TryGetValue(nodeId, out double[] location))
                {
                    throw new FormatException(
                        "Line " + record.LineNumber + ": element " + record.ElementId + " references undefined node " + nodeId + ".");
                }
                nodalLocations[i] = location;
            }

            switch (record.ElementType)
            {
                case "Q4":
                    return new Node4Element2D(material, record.NodeIds, thickness, nodalLocations);
                case "Q8":
                    return new Node8Element2D(material, record.NodeIds, thickness, nodalLocations);
                default:
                    //Unreachable: element type was already validated in ParseElementLine.
                    throw new FormatException("Line " + record.LineNumber + ": unsupported element type \"" + record.ElementType + "\".");
            }
        }

        private static int ParseInt(string field, int lineNumber, string fieldName)
        {
            if (!int.TryParse(field, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException("Line " + lineNumber + ": could not parse " + fieldName + " \"" + field + "\" as an integer.");
            }
            return value;
        }

        private static double ParseDouble(string field, int lineNumber, string fieldName)
        {
            if (!double.TryParse(field, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                throw new FormatException("Line " + lineNumber + ": could not parse " + fieldName + " \"" + field + "\" as a number.");
            }
            return value;
        }
    }
}
