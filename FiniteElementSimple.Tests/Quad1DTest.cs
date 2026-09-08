/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 4/1/2019
 * Time: 1:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using System.Collections.Generic;
using FiniteElementSimple;
using FiniteElementSimple.Materials;
using FiniteElementSimple.BoundaryConditions;

namespace FiniteElementSimple.Tests
{
    [TestFixture]
    public class Quad1DTest
    {
        double acceptablePrecision = 0.001;
        Linear1DConsecutiveElementProblem mylinconsEl;

        [Test]
        public void TestKMatrix()
        {
            List<Assembly> myAss = createAssembly();
            myAss[0].Solve();
            double c = Math.Pow(10.0, 7.0) * 0.6 / 3.0 / 21.0;
            Assert.AreEqual(myAss[0].lElements[0].K[0,0], c*7.0, acceptablePrecision); //this is where the bc is applied
            Assert.AreEqual(myAss[0].lElements[0].K[2,0], 1.0*c, acceptablePrecision);
            Assert.AreEqual(myAss[0].lElements[0].K[2,0], 1.0*c, acceptablePrecision);
            Assert.AreEqual(myAss[0].lElements[0].K[2,2], c*7.0, acceptablePrecision);
            Assert.AreEqual(myAss[0].lElements[0].K[1,2], -8.0*c, acceptablePrecision);
            Assert.AreEqual(myAss[0].lElements[0].K[1,1], 16.0*c, acceptablePrecision);
        }

        [Test]
        public void TestFVector()
        {
            List<Assembly> myAss = createAssembly();
            myAss[0].Solve();
            //Consistent nodal load for a uniform distributed load on a 3-node quadratic element is q*L/6*{1,4,1}
            Assert.AreEqual(myAss[0].lElements[0].F[0], 24.275543479133272, acceptablePrecision); 
            mylinconsEl.plotOutputAlongX(10, 2);
            mylinconsEl.plotOutputAlongX(10, 0);
        }

        public List<Assembly> createAssembly(){
            //Inputs
            int [] n_elements = {2};
            double length = 42;
            double area = 0.6;
            double E = Math.Pow(10.0, 7.0);

            //Create the BCs (these can be out of the loop because node 0 is always node 0
            List<BC> lLoads = new List<BC>();
            List<BC> lBCs = new List<BC>();
            lBCs.Add(new BC(0,0));
            SurfaceTraction1D st1D = new SurfaceTraction1D(new double[]{10.5*0.2836*30*30/32.2/12.0}, 1.0, 2);
            InitialStrain e0 = new InitialStrainDt_1D(new[]{0.0}, 20.0);
            LinElastic1D myMaterial = new LinElastic1D(E);

            mylinconsEl = new Linear1DConsecutiveElementProblem(false, n_elements,	length, area, E,
                                                                                               lLoads, lBCs, myMaterial, st1D, e0);
            return mylinconsEl.lAssembly;
        }
    }
}
