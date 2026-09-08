/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 3/7/2019
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RandomMath;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Elements
{
	/// <summary>
	/// Description of Quadratic1DElement.
	/// </summary>
	public class Node3Element1D:Element1D
	{
		protected double x1;
		protected double x2;
		protected double x3;
		public double length;
		
		public Node3Element1D(Material elementMaterial, int [] localToGlobalConnectivity,
							double area, double x1, double x2, double x3)
			:base(elementMaterial, localToGlobalConnectivity, area, 2)
		{
			this.x1 = x1;
			this.x2 = x2;
			this.x3 = x3;
			length = x3-x1;
		}
		
		public override double [,] ShapeFunction(double xi, double eta, double zeta){
			
			return N_Base(xi,-1.0, 0.0, 1.0);
		}
		
		public override double [] GlobalXPosition(double xi, double eta, double zeta){
			double [] nodalX = new double[]{x1, x2, x3};
			double [] X1d = RandomMath.MatrixMath.Multiply(ShapeFunction(xi, eta, zeta), nodalX);
			return new double[]{X1d[0], 0.0, 0.0};
		}
		
		public override double [,] B(double xi, double eta, double zeta){
			
			double [,] J_inv = MatrixMath.InvertMatrix(J(xi, eta, zeta));
			
			return MatrixMath.ScalarMultiply(J_inv[0,0], dNdxi(xi));
		}
		
		public override double Det_Of_J(double xi, double eta, double zeta){

			return RandomMath.MatrixMath.Determinant(J(xi, eta, zeta));
		}
		
		public override double [,] J(double xi, double eta, double zeta){
			
			return RandomMath.MatrixMath.Multiply(dNdxi(xi), new double[,]{{x1},{x2},{x3}});
		}
				
		private double [,] N_Base(double x, double x_1, double x_2, double x_3){
			double x13=x_1-x_3;
			double x12=x_1-x_2;
			double x23=x_2-x_3;
			
			double [,] N = new double[,]{{(1.0 / (x12*x13)) * (x_3-x) * (x_2-x),
				(-1.0 / (x12*x23)) * (x_1-x) * (x_3-x), 
				(1.0 / (x13*x23)) * (x_1-x) * (x_2-x)}};
			
			return N;
		}
		
		private double [,] dNdxi(double xi){
			
			return new double[,]{{-0.5 * (1.0 - 2.0 * xi),
				-2.0 * xi, 
				0.5 * (1.0 + 2.0 * xi)}};
		}
	}
}