/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 4/25/2019
 * Time: 9:20 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using RandomMath;
using System.Collections.Generic;
using FiniteElementSimple;
using FiniteElementSimple.Elements;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Tests
{

    [TestFixture]
    public class Quadratic8NodeTests
    {
        double acceptablePrecision = 0.00001;

        [Test]
        public void TestNAddsToOneEverywhere()
        {
            int nDivisions = 20;

            LinElastic2DPlaneStress myMat = new LinElastic2DPlaneStress(70000, 0.33);
            Node8Element2D myQuad = new Node8Element2D(myMat, new int[]{1,2,3,4,5,6,7,8}, 1.0, new double[][]{ new double[]{-1,-1}, new double[]{1,-1}, new double[]{1,1},
                                                                        new double[]{-1,1}, new double[]{0,-1}, new double[]{1,0}, new double[]{0,1}, new double[]{-1,0}});

            for (int i = 0; i < nDivisions + 1.0; i++) {
                for (int j = 0; j < nDivisions + 1.0; j++) {

                    double tempXi = -1.0 + 2.0/(nDivisions)*i;
                    double tempEta = -1.0 + 2.0/(nDivisions)*j;
                    double [,] N = myQuad.ShapeFunction(tempXi, tempEta, 0.0);
                    double sum = 0.0;
                    for (int k = 0; k < N.GetLength(1); k++) {
                        sum += N[0,k];
                    }

                    //Add if statement just so that I can put a breakpoint and know when the test doesn't run
                    if (Math.Abs( 1.0-sum) > acceptablePrecision) {
                            bool flag;
                    }
                    Assert.AreEqual(1.0, sum, acceptablePrecision);

                }
            }
        }

        [Test]
        public void TestdNdxiWithNumericalDerivative()
        {
            int nDivisions = 20;
            double delta = 0.000001;

            LinElastic2DPlaneStress myMat = new LinElastic2DPlaneStress(70000, 0.33);
            Node8Element2D myQuad = new Node8Element2D(myMat, new int[]{1,2,3,4,5,6,7,8}, 1.0, new double[][]{ new double[]{-1,-1}, new double[]{1,-1}, new double[]{1,1},
                                                                        new double[]{-1,1}, new double[]{0,-1}, new double[]{1,0}, new double[]{0,1}, new double[]{-1,0}});

            for (int i = 0; i < nDivisions + 1.0; i++) {
                for (int j = 0; j < nDivisions + 1.0; j++) {

                    double tempXi = -1.0 + 2.0/(nDivisions)*i;
                    double tempEta = -1.0 + 2.0/(nDivisions)*j;

                    double [,] dNdxi_Programmed = myQuad.DNdxi(tempXi, tempEta, 0.0);

                    //Find dNdxi
                    double [,] N = myQuad.ShapeFunction(tempXi, tempEta, 0.0);
                    double [,] N_xi_plus = myQuad.ShapeFunction(tempXi+delta, tempEta, 0.0);
                    double [,] N_eta_plus = myQuad.ShapeFunction(tempXi, tempEta+delta, 0.0);

                    double [,] dNdxi_numeric = MatrixMath.ScalarMultiply(1.0/delta, MatrixMath.Subtract(N_xi_plus, N));
                    double [,] dNdeta_numeric = MatrixMath.ScalarMultiply(1.0/delta, MatrixMath.Subtract(N_eta_plus, N));

                    for (int k = 0; k < N.GetLength(1); k++) {
                        Assert.AreEqual(dNdxi_Programmed[0,k], dNdxi_numeric[0,k], acceptablePrecision);
                        Assert.AreEqual(dNdxi_Programmed[1,k], dNdeta_numeric[0,k], acceptablePrecision);
                        Assert.AreEqual(dNdxi_Programmed[2,k], dNdxi_numeric[1,k], acceptablePrecision);
                        Assert.AreEqual(dNdxi_Programmed[3,k], dNdeta_numeric[1,k], acceptablePrecision);
                    }
                }
            }
        }

        [Test]
        public void TestAssemblyEasy()
        {
            //Make local and global K the same, check that it's indeed the same
            LinElastic2DPlaneStress myMat = new LinElastic2DPlaneStress(70000, 0.33);
            Node8Element2D myQuad = new Node8Element2D(myMat, new int[]{1,2,3,4,5,6,7,8}, 1.0, new double[][]{ new double[]{-1,-1}, new double[]{1,-1}, new double[]{1,1},
                                                                        new double[]{-1,1}, new double[]{0,-1}, new double[]{1,0}, new double[]{0,1}, new double[]{-1,0}});
            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            List<Element> lElements = new List<Element>();
            lElements.Add(myQuad);

            //Fully clamp bottom cornor
            //lBCs.Add(new BC(1, 0, 2, 0.0));
            //lBCs.Add(new BC(1, 1, 2, 0.0));

            //Pull upper right Corner
            //lLoads.Add(new BC(3, 2, 2, 1.5));

            Assembly myAssembly = new LinearAssembly(lElements, lLoads, lBCs, 2);

            myAssembly.AssembleLocalKandF();

            for (int i = 0; i < myAssembly.GlobalK.GetLength(0); i++) {
                for (int j = 0; j < myAssembly.GlobalK.GetLength(1); j++) {

                    if (Math.Abs( myAssembly.GlobalK[i,j]-myQuad.K[i,j]) > acceptablePrecision) {
                            bool flag;
                    }
                    Assert.AreEqual(myAssembly.GlobalK[i,j], myQuad.K[i,j], acceptablePrecision);
                }
            }
        }

        [Test]
        public void TestKMatrix_EricsExample()
        {
            //Make local and global K the same, check that it's indeed the same
            LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(43, 0.3);
            double thickness = 1.3;

            int [][] ConnectivityMatrix = {new int[]{1,2,3,4,5,6,7,8}};

            double [][] NodalLocations = new double[][]{
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

            //Create the BCs (these can be out of the loop because node 0 is always node 0
            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            List<Element> lElements = new List<Element>();

            lElements.Add(myQuad);

            Assembly myAssembly = new LinearAssembly(lElements, lLoads, lBCs, 2);

            myAssembly.AssembleLocalKandF();


            double [,] J = myQuad.J(0.2, -0.3, 0.0);

            double [,] B = myQuad.B(0.2, -0.3, 0.0);

            double [,] D = myMaterial.D(0.2, -0.3, 0.0);

            double [,] N = myQuad.ShapeFunction(0.2, -0.3, 0.0);

            double [,] dNdxi = myQuad.DNdxi(0.2, -0.3, 0.0);

            Assert.AreEqual(myAssembly.GlobalK[0,0], 172.5523, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[0,1], 20.568, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[0,2], 69.3753, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[0,3], 2.3441, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[1,1], 79.3010, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[1,2], 1.8322, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[1,3], 38.9118, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[1,4], 7.6495, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[2,2], 66.4961, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[2,3], -14.7191, 0.001);
            Assert.AreEqual(myAssembly.GlobalK[2,4], 27.6401, 0.001);
        }
    }
}
