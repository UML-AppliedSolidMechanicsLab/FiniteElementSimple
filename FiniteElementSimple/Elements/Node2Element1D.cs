/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/25/2019
 * Time: 11:11 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Elements
{
	/// <summary>
	/// Description of LinearElement1D.
	/// </summary>
	public class Node2Element1D:Element1D
	{
		protected double x1;
		protected double x2;
		public double length;
		
		public Node2Element1D(Material elementMaterial, int [] localToGlobalConnectivity,
		                    double area, double x1, double x2)
			:base(elementMaterial, localToGlobalConnectivity, area, 1)
		{
			this.x1 = x1;
			this.x2 = x2;
			length = x2-x1;
		}
		
		public override double [,] ShapeFunction(double xi, double eta, double zeta){
			
			return new double[,]{{0.5*(1.0-xi), 0.5*(1.0+xi)}};
		}
		
		public override double [] GlobalXPosition(double xi, double eta, double zeta){
			double [] nodalX = new double[]{x1, x2};
			double [] X1d = RandomMath.MatrixMath.Multiply(ShapeFunction(xi, eta, zeta), nodalX);
			return new double[]{X1d[0], 0.0, 0.0};
		}
		
		public override double [,] B(double xi, double eta, double zeta){
			
			double J_inv = 2.0 / length;
			
			return new double[,]{{J_inv * -0.5, J_inv * 0.5}};
		}
		
		public override double Det_Of_J(double xi, double eta, double zeta){

			return length / 2.0;
		}

		public override double [,] J(double xi, double eta, double zeta){

			return new double[,]{{length / 2.0}};
		}
		
	}
}
