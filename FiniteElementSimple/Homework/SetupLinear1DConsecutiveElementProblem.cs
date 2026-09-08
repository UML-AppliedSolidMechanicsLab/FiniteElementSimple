/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/28/2019
 * Time: 3:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SinglePlotZedGraph;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple
{
	/// <summary>
	/// Description of SetupLinear1DConsecutiveElementProblem.
	/// </summary>
	public class Linear1DConsecutiveElementProblem
	{
		public List<Assembly> lAssembly;
		public int nDOF;
		
		public Linear1DConsecutiveElementProblem(bool isLinearelement, int [] n_elements,	double length, double area, double E,
		                                         List<BC> lLoads, List<BC> lBCs, LinElastic1D myMaterial, SurfaceTraction1D st1D, InitialStrain e0)
		{
			nDOF = (isLinearelement ? 2 : 3);
			
			//Loop through each model I want to look at (each mesh density)
			
			lAssembly = new List<Assembly>();
			
			for (int i = 0; i < n_elements.Length; i++) {
				
				int [][] ConnMatrix = CreateConsecutiveConnectivity(n_elements[i]);
				double elementLength = length / n_elements[i];
				List<Element> lElements = new List<Element>();
				
				//loop through each element and add the element object
				for (int j = 0; j < n_elements[i]; j++) {
					if (nDOF == 2) {
						lElements.Add(new Node2Element1D(myMaterial, ConnMatrix[j], area, j*elementLength, (j+1)*elementLength));
					}
					else{
						lElements.Add(new Node3Element1D(myMaterial, ConnMatrix[j], area, j*elementLength, 
						                                     (j+0.5)*elementLength, (j+1)*elementLength));
					}
					lElements[lElements.Count-1].lSurfaceTractions.Add(st1D);
					lElements[lElements.Count-1].lInitialStrain.Add(e0);
				}
				
				//Clamp the end
				//lBCs[lBCs.Count-1].dofNumber = lElements[lElements.Count-1].localToGlobalConnectivity[lElements[lElements.Count-1].localToGlobalConnectivity.Length-1];
				List <BC> lBCsCopy = new List<BC>();
				lBCsCopy.Add(lBCs[0]);
				//lBCsCopy.Add(new BC(lBCs[1].dofNumber, lBCs[1].magnitude));
				
				//Now create assembly
				lAssembly.Add(new Assembly(lElements, lLoads, lBCsCopy, 1));
				
			}
		}
		public void Solve(){
			//Now solve the assembly
			foreach (Assembly ass in lAssembly) {
				
				ass.Solve();
			}
		}
		
		private  int [][] CreateConsecutiveConnectivity(int nElements){
			
			int [][] connMatrix = new int[nElements][];
			
			for (int i = 0; i < nElements; i++) {
				
				connMatrix[i] = new int[nDOF];
				for (int j = 0; j < nDOF; j++) {
					connMatrix[i][j] = (nDOF-1)*i + j;
				}
			}
			return connMatrix;
		}
		
		public void plotOutputAlongX(int nPointsPerElement, int u0_e1_s2){
			
			List<string> lLabels = new List<string>();
			List<double []> lYData = new List<double[]>();
			List<double []> lXData = new List<double[]>();
			string yTitle = "";
			
			//Compile the data
			foreach (Assembly ass in lAssembly) {
				double [] tempX = new double[ass.lElements.Count * nPointsPerElement + 1];
				double [] tempY = new double[ass.lElements.Count * nPointsPerElement + 1];
				lLabels.Add(ass.lElements.Count.ToString() + " elements");
				
				for (int i = 0; i < ass.lElements.Count; i++) {
					for (int j = 0; j < nPointsPerElement; j++) {
						double tempXi = -1.0 + 2.0/(nPointsPerElement + 1.0)*j;
						tempX[i * nPointsPerElement + j] = ass.lElements[i].GlobalXPosition(tempXi,0.0,0.0)[0];
						
						switch (u0_e1_s2) {
							case 0:
								tempY[i * nPointsPerElement + j] = ass.lElements[i].Displacement(tempXi,0.0,0.0)[0];
								break;
							case 1:
								tempY[i * nPointsPerElement + j] = ass.lElements[i].Strain(tempXi,0.0,0.0)[0];
								break;
							case 2:
								tempY[i * nPointsPerElement + j] = ass.lElements[i].Stress(tempXi,0.0,0.0)[0];
								break;
						}
					}
					
				}
				tempX[tempX.Length-1] = ass.lElements[ass.lElements.Count-1].GlobalXPosition(1.0, 0.0, 0.0)[0];
				
				switch (u0_e1_s2) {
					case 0:
						tempY[tempY.Length-1] = ass.lElements[ass.lElements.Count-1].Displacement(1.0,0.0,0.0)[0];
						yTitle = "u (mm)";
						break;
					case 1:
						tempY[tempY.Length-1] = ass.lElements[ass.lElements.Count-1].Strain(1.0,0.0,0.0)[0];
						yTitle = "epsilon (MPa)";
						break;
					case 2:
						tempY[tempY.Length-1] = ass.lElements[ass.lElements.Count-1].Stress(1.0,0.0,0.0)[0];
						yTitle = "sigma (MPa)";
						break;
				}
				
				lYData.Add(tempY);
				lXData.Add(tempX);
			}
			
			SinglePlotForm myForm = new SinglePlotForm("results", "x (mm)", yTitle, lLabels, lXData, lYData);
			myForm.Activate();
			myForm.ShowDialog();
			
		}
	}
}
