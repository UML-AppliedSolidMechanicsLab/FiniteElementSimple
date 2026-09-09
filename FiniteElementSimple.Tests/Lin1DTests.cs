/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/28/2019
 * Time: 3:51 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using System.Collections.Generic;
using FiniteElementSimple;
using FiniteElementSimple.Elements;
using FiniteElementSimple.BoundaryConditions;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Tests
{
    /// <summary>
    /// This runs example 3.11 in Logan's "A first course in FE Method" book
    /// </summary>
    [TestFixture]
    public class Lin1DTests
    {

        double acceptablePrecision = 0.001;

        [Test]
        public void TestKMatrix()
        {
            List<Assembly> myAssembly = createAssembly();
            myAssembly[0].Solve();
            //Assert.AreEqual(myAss[0].GlobalK[0,0], 1000000.0, acceptablePrecision); //this is where the bc is applied
            Assert.AreEqual(myAssembly[0].GlobalK[0,1], -1000000.0, acceptablePrecision);
            Assert.AreEqual(myAssembly[0].GlobalK[1,0], -1000000.0, acceptablePrecision);
            Assert.AreEqual(myAssembly[0].GlobalK[1,1], 1000000.0, acceptablePrecision);

        }

        [Test]
        public void TestLocalFMatrix()
        {
            List<Assembly> myAssembly = createAssembly();
            myAssembly[0].Solve();
            Assert.AreEqual(myAssembly[0].lElements[0].F[0], 9000.0, acceptablePrecision);
            Assert.AreEqual(myAssembly[0].lElements[0].F[0], 9000.0, acceptablePrecision);

        }

        /// <summary>
        /// Example 3.8 in the Chandrupatla book
        /// </summary>
        [Test]
        public void TestLocalFMatrix_initialStrain()
        {
            //Inputs

            //Create the BCs (these can be out of the loop because node 0 is always node 0
            List<BC> lLoads = new List<BC>();
            lLoads.Add(new BC(1, 300000.0));
            List<BC> lBCs = new List<BC>();
            lBCs.Add(new BC(0,0.0));
            lBCs.Add(new BC(2,0.0));
            InitialStrain e0_1 = new InitialStrainDt_1D(new double[]{60}, 23.0E-6);
            InitialStrain e0_2 = new InitialStrainDt_1D(new double[]{60}, 11.7E-6);
            LinElastic1D myMaterial1 = new LinElastic1D(70000.0);
            LinElastic1D myMaterial2 = new LinElastic1D(200000.0);

            List <Element> lEl = new List <Element>();
            //localToGlobalConnectivity uses a 1-index system (see Assembly.AssembleLocalKandF)
            lEl.Add( new Node2Element1D(myMaterial1, new int[]{1,2}, 900.0, 0.0, 200.0));
            lEl.Add( new Node2Element1D(myMaterial2, new int[]{2,3}, 1200.0, 200.0, 500.0));

            lEl[0].lInitialStrain.Add(e0_1);
            lEl[1].lInitialStrain.Add(e0_2);

            Assembly myAssembly = new LinearAssembly(lEl, lLoads, lBCs, 1);

            myAssembly.Solve();
            //Thermal (initial strain) force for a constant-strain rod is F = A*E*alpha*dT
            Assert.AreEqual(myAssembly.lElements[0].F[0], -86.94E3, acceptablePrecision);
            Assert.AreEqual(myAssembly.lElements[0].F[1], 86.94E3, acceptablePrecision);
            Assert.AreEqual(myAssembly.lElements[1].F[0], -168.48E3, acceptablePrecision);
            Assert.AreEqual(myAssembly.lElements[1].F[1], 168.48E3, acceptablePrecision);
            Assert.AreEqual(myAssembly.GlobalF[1], 218.46E3, acceptablePrecision);

        }

        public List<Assembly> createAssembly(){
            //Inputs
            int [] n_elements = {1};
            double length = 60;
            double area = 2.0;
            double E = 30000000.0;

            //Create the BCs (these can be out of the loop because node 0 is always node 0
            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            lBCs.Add(new BC(0,0));
            SurfaceTraction1D st1D = new SurfaceTraction1D(new double[]{600, -10}, 1.0, 1);
            InitialStrain e0 = new InitialStrainDt_1D(new[]{0.0}, 20.0);
            LinElastic1D myMaterial = new LinElastic1D(E);

            Linear1DConsecutiveElementProblem mylinconsEl = new Linear1DConsecutiveElementProblem(true, n_elements,	length, area, E,
                                                                                               lLoads, lBCs, myMaterial, st1D, e0);
            return mylinconsEl.lAssembly;
        }
    }
}
