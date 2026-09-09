/*
 * Automated test for the simple text-based FE input-file reader (FiniteElementSimple.IO.InputFileReader).
 * Verifies that reading and solving the example input file produces the same displacement results as the
 * existing hard-coded single-Q8-element problem (see VtuWriterTests.TestWriteSolvedQ8Model /
 * Quadratic8NodeTests.TestKMatrix_EricsExample).
 */
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using FiniteElementSimple;
using FiniteElementSimple.Elements;
using FiniteElementSimple.Materials;
using FiniteElementSimple.BoundaryConditions;
using FiniteElementSimple.IO;

namespace FiniteElementSimple.Tests
{
    [TestFixture]
    public class InputFileReaderTests
    {
        private double acceptablePrecision = 0.00001;

        /// <summary>
        /// Locates FiniteElementSimple/ExampleInputFiles/SingleQ8Element.fem by walking up from the test
        /// assembly's output directory to the repository/solution root.
        /// </summary>
        private static string GetExampleInputFilePath()
        {
            DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "FiniteElementSimple", "ExampleInputFiles", "SingleQ8Element.fem");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }

            throw new FileNotFoundException("Could not locate SingleQ8Element.fem by walking up from " + AppContext.BaseDirectory);
        }

        private static Assembly BuildHardCodedSingleQ8Assembly(out Node8Element2D myQuad)
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

            Assembly myAssembly = new LinearAssembly(lElements, lLoads, lBCs, 2);
            myAssembly.Solve();
            return myAssembly;
        }

        [Test]
        public void TestReadAndSolve_MatchesHardCodedSingleQ8Model()
        {
            string filePath = GetExampleInputFilePath();

            Assembly assemblyFromFile = InputFileReader.Read(filePath);
            assemblyFromFile.Solve();

            Assembly hardCodedAssembly = BuildHardCodedSingleQ8Assembly(out Node8Element2D hardCodedQuad);

            Assert.AreEqual(hardCodedAssembly.GlobalQ.Length, assemblyFromFile.GlobalQ.Length);
            for (int i = 0; i < hardCodedAssembly.GlobalQ.Length; i++)
            {
                Assert.AreEqual(hardCodedAssembly.GlobalQ[i], assemblyFromFile.GlobalQ[i], acceptablePrecision,
                    "Mismatch at global DOF " + i);
            }
        }

        [Test]
        public void TestRead_UnknownSectionKeyword_ThrowsWithLineNumber()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "InputFileReaderTest_BadSection_" + Guid.NewGuid().ToString("N") + ".fem");
            try
            {
                File.WriteAllLines(tempFilePath, new[] { "*NODES", "1, 0, 0", "*BOGUS", "1, 2, 3" });

                FormatException ex = Assert.Throws<FormatException>(() => InputFileReader.Read(tempFilePath));
                Assert.That(ex.Message, Does.Contain("Line 3"));
                Assert.That(ex.Message, Does.Contain("BOGUS"));
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestRead_DuplicateNodeNumber_Throws()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "InputFileReaderTest_DupNode_" + Guid.NewGuid().ToString("N") + ".fem");
            try
            {
                File.WriteAllLines(tempFilePath, new[]
                {
                    "*NODES",
                    "1, 0, 0",
                    "1, 1, 1",
                });

                FormatException ex = Assert.Throws<FormatException>(() => InputFileReader.Read(tempFilePath));
                Assert.That(ex.Message, Does.Contain("duplicate node number 1"));
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestRead_InvalidNodeReferenceInElement_Throws()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "InputFileReaderTest_BadNodeRef_" + Guid.NewGuid().ToString("N") + ".fem");
            try
            {
                File.WriteAllLines(tempFilePath, new[]
                {
                    "*MATERIALS",
                    "1, PLANESTRESS, 43, 0.3",
                    "*NODES",
                    "1, 0, 0",
                    "2, 1, 0",
                    "3, 1, 1",
                    "4, 0, 1",
                    "*THICKNESS",
                    "1.0",
                    "*ELEMENTS",
                    "1, Q4, 1, 1, 2, 3, 99",
                });

                FormatException ex = Assert.Throws<FormatException>(() => InputFileReader.Read(tempFilePath));
                Assert.That(ex.Message, Does.Contain("undefined node 99"));
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestRead_UnsupportedElementType_Throws()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "InputFileReaderTest_BadElemType_" + Guid.NewGuid().ToString("N") + ".fem");
            try
            {
                File.WriteAllLines(tempFilePath, new[]
                {
                    "*MATERIALS",
                    "1, PLANESTRESS, 43, 0.3",
                    "*NODES",
                    "1, 0, 0",
                    "2, 1, 0",
                    "3, 1, 1",
                    "*THICKNESS",
                    "1.0",
                    "*ELEMENTS",
                    "1, TRI3, 1, 1, 2, 3",
                });

                FormatException ex = Assert.Throws<FormatException>(() => InputFileReader.Read(tempFilePath));
                Assert.That(ex.Message, Does.Contain("unsupported element type"));
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestRead_UnsupportedMaterialType_Throws()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "InputFileReaderTest_BadMatType_" + Guid.NewGuid().ToString("N") + ".fem");
            try
            {
                File.WriteAllLines(tempFilePath, new[]
                {
                    "*MATERIALS",
                    "1, ORTHOTROPIC, 43, 0.3",
                });

                FormatException ex = Assert.Throws<FormatException>(() => InputFileReader.Read(tempFilePath));
                Assert.That(ex.Message, Does.Contain("unsupported material type"));
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }
    }
}
