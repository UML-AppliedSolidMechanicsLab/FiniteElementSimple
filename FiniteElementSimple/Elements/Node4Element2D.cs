/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 4/18/2019
 * Time: 6:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RandomMath;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Elements
{
	/// <summary>
	/// Description of 4NodedElement2D:
	/// Nodes start .
	/// </summary>
	public class Node4Element2D : QuadraticElement2D
	{
		#region private Properties

		#endregion

		public Node4Element2D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
									 double thickness, double[][] nodalLocations)
			: base(elementMaterial, localToGlobalConnectivity_1index, thickness, nodalLocations, 2, 2)
		{

		}

		public override double[,] ShapeFunction(double xi, double eta, double zeta)
		{
			double N1 = 0.25 * (xi - 1.0) * (eta - 1.0); 
			double N2 = -0.25 * (xi + 1.0) * (eta - 1.0);  
			double N3 = 0.25 * (xi + 1.0) * (eta + 1.0); 
			double N4 = -0.25 * (xi - 1.0) * (eta + 1.0);
			return new double[,]{{N1, 0.0, N2, 0.0, N3, 0.0, N4, 0.0},
				{0.0, N1, 0.0, N2, 0.0, N3, 0.0, N4}};
		}

		public override double[,] DNdxi(double xi, double eta, double zeta)
		{

			double dN1dxi = -0.25 * (eta - 1);
			double dN1deta = -0.25 * (xi + 1);

			double dN2dxi = 0.25 * (eta + 1);
			double dN2deta = 0.25 * (xi + 1);

			double dN3dxi = -0.25 * (eta + 1);
			double dN3deta = -0.25 * (xi - 1);

			double dN4dxi = 0.25 * (eta - 1);
			double dN4deta = 0.25 * (xi - 1);

			return new double[,]{{dN1dxi, 0.0, dN2dxi, 0.0, dN3dxi, 0.0, dN4dxi, 0.0},
				{dN1deta, 0.0, dN2deta, 0.0, dN3deta, 0.0, dN4deta, 0.0},
				{0.0, dN1dxi, 0.0, dN2dxi, 0.0, dN3dxi, 0.0, dN4dxi},
			{0.0, dN1deta, 0.0, dN2deta, 0.0, dN3deta, 0.0, dN4deta}};
		}
	}
}

