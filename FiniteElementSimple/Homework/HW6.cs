/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 4/25/2019
 * Time: 12:59 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using FiniteElementSimple.Plotting;
using System.Collections.Generic;
using FiniteElementSimple.BoundaryConditions;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Homework
{
	/// <summary>
	/// Description of HW6.
	/// </summary>
	public static class HW6
	{
		public static void HW6Setup(){
			
			//Inputs
			int [] n_elements = {1,2,4,8,16,32};
			double length = 15;
			double area = 4.5;
			double E = 70000.0;
			
			//Create the BCs (these can be out of the loop because node 0 is always node 0
			List<BC> lLoads = new List<BC>();
			List<BC> lBCs = new List<BC>();
			lBCs.Add(new BC(0,0));
			//lBCs.Add(new BC(0,0));
			//SurfaceTraction1D st1D = new SurfaceTraction1D(new double[]{1.0, 2.3}, 2.0, 2);
			SurfaceTraction1D st1D = new SurfaceTraction1D(new double[]{1.0,2.3}, 2.0, 2);
			
			//SurfaceTraction1D st1D = new SurfaceTraction1D(new double[]{0.0}, 2.0, 2);
			//InitialStrain e0 = new InitialStrainDt_1D(new double[]{50.0, 15.0, -10.0}, 23e-6);
			InitialStrain e0 = new InitialStrainDt_1D(new double[]{0.0}, 0.0);
			
			LinElastic1D myMaterial = new LinElastic1D(E);
			
			Linear1DConsecutiveElementProblem mylinconsEl = new Linear1DConsecutiveElementProblem(false, n_elements,	length, area, E,
			                                                                                      lLoads, lBCs, myMaterial, st1D, e0);
			
			//Linear1DConsecutiveElementProblem mylinconsEl = new Linear1DConsecutiveElementProblem(true, n_elements,	length, area, E,
			//                                                                                   lLoads, lBCs, st1D, myMaterial);
			//Linear1DConsecutiveElementProblem mylinconsEl = new Linear1DConsecutiveElementProblem(false, n_elements, length, area, E,
			//                                                                                   lLoads, lBCs, st1D, myMaterial);
			
			
			mylinconsEl.Solve();
			mylinconsEl.plotOutputAlongX(10,0);
			mylinconsEl.plotOutputAlongX(10,1);
			mylinconsEl.plotOutputAlongX(10,2);
			
			
		}
	}
}
