/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 4/25/2019
 * Time: 1:01 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using FiniteElementSimple.Elements;
using FiniteElementSimple.BoundaryConditions;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Homework
{
	/// <summary>
	/// Description of HW10.
	/// </summary>
	public static class Homework10
	{
		public static void RunHW10(){
			
			double delta = 0.1;
			LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(70000, 0.33);
			double thickness = 1.0;
			
			int [][] ConnectivityMatrix = {new int[]{9,11,3,1,10,7,2,6},
				new int[]{11,13,5,3,12,8,4,7},
				new int[]{16,18,11,9,17,15,10,14},
				new int[]{18,22,24,11,19,23,20,15},
				new int[]{24,26,13,11,25,21,12,20}};
			
			double [][] NodalLocations = new double[][]{
				new double[]{0,6},
				new double[]{1.5,6},
				new double[]{3,6},
				new double[]{4.5,6},
				new double[]{6,6},
				new double[]{0,4.5},
				new double[]{3,4.5},
				new double[]{6,4.5},
				new double[]{0,3},
				new double[]{1.5,3},
				new double[]{3,3},
				new double[]{4.5,3},
				new double[]{6,3},
				new double[]{0,1.5},
				new double[]{3,1.5},
				new double[]{0,0},
				new double[]{1.5,0},
				new double[]{3,0},
				new double[]{3.5,0},
				new double[]{3.792893219,2.207106781},
				new double[]{6,2.5},
				new double[]{4,0},
				new double[]{4.152240935,0.765366865},
				new double[]{4.585786438,1.414213562},
				new double[]{5.234633135,1.847759065},
				new double[]{6,2}};

			//Create the BCs (these can be out of the loop because node 0 is always node 0
			List<BC> lLoads = new List<BC>();
			List<BC> lBCs = new List<BC>();
			List<Element> lElements = new List<Element>();
			
			//Right Edge
			lBCs.Add(new BC(5,0,2,0));
			lBCs.Add(new BC(8,0,2,0));
			lBCs.Add(new BC(13,0,2,0));
			lBCs.Add(new BC(21,0,2,0));
			lBCs.Add(new BC(26,0,2,0));
			//Bottom Edge
			lBCs.Add(new BC(16,1,2,0));
			lBCs.Add(new BC(17,1,2,0));
			lBCs.Add(new BC(18,1,2,0));
			lBCs.Add(new BC(19,1,2,0));
			lBCs.Add(new BC(22,1,2,0));
			//Top Edge
			lBCs.Add(new BC(1,1,2,delta));
			lBCs.Add(new BC(2,1,2,delta));
			lBCs.Add(new BC(3,1,2,delta));
			lBCs.Add(new BC(4,1,2,delta));
			lBCs.Add(new BC(5,1,2,delta));
			
			//Create nodal location arrays
			
			for (int i = 0; i < ConnectivityMatrix.Length; i++) {
				double [][] localNodalCoorArray = new double[ConnectivityMatrix[i].Length][];
				for (int j = 0; j < ConnectivityMatrix[i].Length; j++) {
					localNodalCoorArray[j] = NodalLocations[ConnectivityMatrix[i][j]-1];
				}
				lElements.Add(new Node8Element2D(myMaterial, ConnectivityMatrix[i], thickness, localNodalCoorArray));
			}
			
			Assembly myAssembly = new Assembly(lElements, lLoads, lBCs, 2);
			myAssembly.Solve();
			
			CreateContourPlot(myAssembly, 25, 25, 0, 0, false);
			CreateContourPlot(myAssembly, 25, 25, 0, 1, false);
			//CreateContourPlot(myAssembly, 50, 50, 1, 0);
			//CreateContourPlot(myAssembly, 50, 50, 1, 1);
			//CreateContourPlot(myAssembly, 50, 50, 1, 2);
			CreateContourPlot(myAssembly, 25, 25, 2, 0, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 1, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 2, true);
			
		}

		public static void RunHW3_Example()
		{

			double delta = 0.1;
			LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(70000, 0.33);
			double thickness = 1.0;

			double[][] NodalLocations = new double[][]{
				new double[]{0,0},
				new double[]{6,0.5},
				new double[]{12,-1},
				new double[]{11,5},
				new double[]{15,8},
				new double[]{6,11},
				new double[]{-1,10},
				new double[]{1,5}};

			int[] ConnectivityMatrix = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
			//Create nodal location arrays

			Node8Element2D myElement = new Node8Element2D(myMaterial, ConnectivityMatrix, thickness, NodalLocations);
			List<Element> myAssembly = new List<Element>();
			myAssembly.Add(myElement);

			double[] q = { 0, 0, 0.1, -0.1, 0.2, -0.3, 0.2, -0.3, 0.2, -0.3, 0.1, -0.1, 0, 0.1, 0, 0 };


			double [,] dNdxi = myElement.DNdxi(0.1, -0.1, 0.0);

			double [,] J = myElement.J(0.1, -0.1, 0.0);

			double[,] B = myElement.B(0.1, -0.1, 0.0);

			Assembly assembly = new Assembly(myAssembly, new List<BC>(), new List<BC>(), 2);

			CreateContourPlot(assembly, 50, 50, 3, 0, false);
		}

		public static void RunHW9(){
			
			double force = 0.0000002;
			LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(700000, 0.4);
			double thickness = 0.01;
			
			int [][] ConnectivityMatrix = {new int[]{1,2,3,4,5,6,7,8}};
			
			/*double [][] NodalLocations = new double[][]{
				new double[]{0,0},
				new double[]{5,0.2},
				new double[]{6,6},
				new double[]{0.5,5},
				new double[]{1,1},
				new double[]{5.1,1.2},
				new double[]{2,5.5},
				new double[]{0.75,1},
				};*/
			
			double [][] NodalLocations = new double[][]{
				new double[]{0,0},
				new double[]{5,0.2},
				new double[]{6,6},
				new double[]{0.5,5},
				new double[]{2,0.1},
				new double[]{5.1,3},
				new double[]{2,5.5},
				new double[]{0.1,3},
				};

			//Create the BCs (these can be out of the loop because node 0 is always node 0
			List<BC> lLoads = new List<BC>();
			List<BC> lBCs = new List<BC>();
			List<Element> lElements = new List<Element>();
			
			//Right Edge
			lBCs.Add(new BC(2,0,2,0.0));
			lBCs.Add(new BC(2,1,2,0.0));
			
			//lBCs.Add(new BC(3,0,2,0.0));
			
			//Load left side
			lLoads.Add(new BC(4,1,2,force));
			
			//Create nodal location arrays
			Node8Element2D myQuad = new Node8Element2D(myMaterial, ConnectivityMatrix[0], thickness, NodalLocations);
			lElements.Add(myQuad);
			
			Assembly myAssembly = new Assembly(lElements, lLoads, lBCs, 2);
			myAssembly.Solve();
			
			CreateContourPlot(myAssembly, 25, 25, 0, 0, false);
			CreateContourPlot(myAssembly, 25, 25, 0, 1, false);
			CreateContourPlot(myAssembly, 25, 25, 2, 0, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 1, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 2, true);
			
		}
		
		public static void RunEricsExample(){
			
			double force = -20.0;
			LinElastic2DPlaneStress myMaterial = new LinElastic2DPlaneStress(70000, 0.33);
			double thickness = 0.01;
			
			int [][] ConnectivityMatrix = {new int[]{1,2,3,4,5,6,7,8}};
			
			double [][] NodalLocations = new double[][]{
				new double[]{0,0},
				new double[]{2,0},
				new double[]{2,2},
				new double[]{0,2},
				new double[]{1,0},
				new double[]{2,1},
				new double[]{1,2},
				new double[]{0,1},
				};

			//Create the BCs (these can be out of the loop because node 0 is always node 0
			List<BC> lLoads = new List<BC>();
			List<BC> lBCs = new List<BC>();
			List<Element> lElements = new List<Element>();
			
			//Right Edge
			lBCs.Add(new BC(2,0,2,0.0));
			lBCs.Add(new BC(2,1,2,0.0));
			
			lBCs.Add(new BC(6,0,2,0.0));
			lBCs.Add(new BC(3,0,2,0.0));
			
			//Load left side
			lLoads.Add(new BC(4,0,2,force));
			lLoads.Add(new BC(8,0,2,force));
			lLoads.Add(new BC(1,0,2,force));
			
			//Create nodal location arrays
			Node8Element2D myQuad = new Node8Element2D(myMaterial, ConnectivityMatrix[0], thickness, NodalLocations);
			lElements.Add(myQuad);
			
			Assembly myAssembly = new Assembly(lElements, lLoads, lBCs, 2);
			myAssembly.Solve();
			
			CreateContourPlot(myAssembly, 25, 25, 0, 0, false);
			CreateContourPlot(myAssembly, 25, 25, 0, 1, false);
			CreateContourPlot(myAssembly, 25, 25, 2, 0, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 1, true);
			CreateContourPlot(myAssembly, 25, 25, 2, 2, true);
			
		}
		
		public static void CreateContourPlot(Assembly myAssembly, int nDivPerElX, int nDivPerElY, int u0_e1_s2_j3, int direction, bool plotDeformedState){
			
			//Initialize lists to hold data: x and y are coordinates, z is the quantity to be plotted (displacement, strain, stress)
			//Lists are arrays that can be resized dynamically
			List<double> lYData = new List<double>();
			List<double> lXData = new List<double>();
			List<double> lZData = new List<double>();
			string zTitle = "";
			
			//Compile the data
			
			//Loop through each element
			for (int i = 0; i < myAssembly.lElements.Count; i++) {
				
				//Loop through the number of divisions within the element in the x direction
				for (int j = 0; j < nDivPerElX + 1; j++) {
					
					//Set the xi value just by incrementing
					double tempXi = -1.0 + 2.0/(nDivPerElX)*j;
					
					//Loop through the number of divisions within the element in the y direction
					for (int k = 0; k < nDivPerElY + 1; k++) {
						
						//Set the eta value just by incrementing
						double tempEta = -1.0 + 2.0/(nDivPerElY)*k;
						
						//Get the global x and y value from the local xi and eta
						double [] tempX = myAssembly.lElements[i].GlobalXPosition(tempXi,tempEta,0.0);
						double tempZ = 0;
						
						//Extrapolate the desired output (defined by the user in the function inputs
						switch (u0_e1_s2_j3) {
							case 0:
								tempZ = myAssembly.lElements[i].Displacement(tempXi,tempEta,0.0)[direction];
								zTitle = "Displacement, U_" + (direction+1);
								break;
							case 1:
								tempZ = myAssembly.lElements[i].Strain(tempXi,tempEta,0.0)[direction];
								zTitle = "Strain, E_" + (direction+1);
								break;
							case 2:
								tempZ = myAssembly.lElements[i].Stress(tempXi,tempEta,0.0)[direction];
								zTitle = "Stress, S_" + (direction+1);
								break;
							case 3:
								tempZ = myAssembly.lElements[i].J(tempXi, tempEta, 0.0)[direction,direction];
								zTitle = "Jacobian, J_" + (direction + 1) + "," + (direction + 1);
								break;
						}
						//If the points should be in the deformed shape, then add the displacements to the coordinates
						if (plotDeformedState) {
							double [] def =  myAssembly.lElements[i].Displacement(tempXi,tempEta,0.0);
							tempX[0]+=def[0];
							tempX[1]+=def[1];
						}
						//Add the x, y, and z values into their lists
						lXData.Add(tempX[0]);
						lYData.Add(tempX[1]);
						lZData.Add(tempZ);
					}
				}
			}
			//Then I make a color scheme and create the plot.  This part will be different for you: I was calling a function I 
			//Had already written
			Color [] colorScheme = new Color[]{Color.Blue, Color.Aqua, Color.LimeGreen, Color.Yellow, Color.Red};
			FiniteElementSimple.Plotting.SinglePlotForm myPlot = new FiniteElementSimple.Plotting.SinglePlotForm(zTitle, lXData.ToArray(), lYData.ToArray(), 
																			 lZData.ToArray(), colorScheme);
			myPlot.Activate();
			myPlot.ShowDialog();
		}
	}
}
