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
	/// Description of Quadratic8NodeElement.
	/// </summary>
	public class Node8Element2D : QuadraticElement2D
	{

		public Node8Element2D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
									 double thickness, double[][] nodalLocations)
			: base(elementMaterial, localToGlobalConnectivity_1index, thickness, nodalLocations, 2, 2)
		{
		}

		public override double[,] ShapeFunction(double xi, double eta, double zeta)
		{
			double N1 = -0.25 * (1.0 - xi) * (1.0 - eta) * (1.0 + eta + xi);
			double N2 = -0.25 * (1.0 + xi) * (1.0 - eta) * (1.0 + eta - xi);
			double N3 = -0.25 * (1.0 + xi) * (1.0 + eta) * (1.0 - eta - xi);
			double N4 = -0.25 * (1.0 - xi) * (1.0 + eta) * (1.0 - eta + xi);
			double N5 = 0.5 * (1.0 - xi) * (1.0 + xi) * (1.0 - eta);
			double N6 = 0.5 * (1.0 + xi) * (1.0 + eta) * (1.0 - eta);
			double N7 = 0.5 * (1.0 - xi) * (1.0 + xi) * (1.0 + eta);
			double N8 = 0.5 * (1.0 - xi) * (1.0 + eta) * (1.0 - eta);
			return new double[,]{{N1, 0.0, N2, 0.0, N3, 0.0, N4, 0.0, N5, 0.0, N6, 0.0, N7, 0.0, N8, 0.0},
				{0.0, N1, 0.0, N2, 0.0, N3, 0.0, N4, 0.0, N5, 0.0, N6, 0.0, N7, 0.0, N8}};
		}

		public override double[,] DNdxi(double xi, double eta, double zeta)
		{

			double dN1dxi = 0.25 * (1.0 - eta) * (xi + eta + 1.0) - 0.25 * (1.0 - eta) * (1.0 - xi);
			double dN1deta = 0.25 * (1.0 - xi) * (xi + eta + 1.0) - 0.25 * (1.0 - eta) * (1.0 - xi);

			double dN2dxi = 0.25 * (1.0 - eta) * (xi + 1.0) - 0.25 * (1.0 - eta) * (-xi + eta + 1.0);
			double dN2deta = 0.25 * (-xi + eta + 1.0) * (xi + 1.0) - 0.25 * (1.0 - eta) * (xi + 1.0);

			double dN3dxi = 0.25 * (eta + 1.0) * (xi + 1.0) - 0.25 * (eta + 1.0) * (-xi - eta + 1.0);
			double dN3deta = 0.25 * (eta + 1.0) * (xi + 1.0) - 0.25 * (-xi - eta + 1.0) * (xi + 1.0);

			double dN4dxi = 0.25 * (eta + 1.0) * (xi - eta + 1.0) - 0.25 * (eta + 1.0) * (1.0 - xi);
			double dN4deta = 0.25 * (eta + 1.0) * (1.0 - xi) - 0.25 * (1.0 - xi) * (xi - eta + 1.0);

			double dN5dxi = 0.5 * (1.0 - eta) * (1.0 - xi) - 0.5 * (1.0 - eta) * (xi + 1.0);
			double dN5deta = -0.5 * (1.0 - xi) * (xi + 1.0);

			double dN6dxi = 0.5 * ((1.0 + eta) * (1.0 - eta));
			double dN6deta = 0.5 * ((1.0 + xi) * (1.0 - eta) - (1.0 + xi) * (1.0 + eta));

			double dN7dxi = 0.5 * (eta + 1.0) * (1.0 - xi) - 0.5 * (eta + 1.0) * (xi + 1.0);
			double dN7deta = 0.5 * (1.0 - xi) * (xi + 1.0);

			double dN8dxi = -0.5 * (1.0 - eta) * (eta + 1.0);
			double dN8deta = 0.5 * (1.0 - eta) * (1.0 - xi) - 0.5 * (eta + 1.0) * (1.0 - xi);

			return new double[,]{{dN1dxi, 0.0, dN2dxi, 0.0, dN3dxi, 0.0, dN4dxi, 0.0, dN5dxi, 0.0, dN6dxi, 0.0, dN7dxi, 0.0, dN8dxi, 0.0},
				{dN1deta, 0.0, dN2deta, 0.0, dN3deta, 0.0, dN4deta, 0.0, dN5deta, 0.0, dN6deta, 0.0, dN7deta, 0.0, dN8deta, 0.0},
				{0.0, dN1dxi, 0.0, dN2dxi, 0.0, dN3dxi, 0.0, dN4dxi, 0.0, dN5dxi, 0.0, dN6dxi, 0.0, dN7dxi, 0.0, dN8dxi},
			{0.0, dN1deta, 0.0, dN2deta, 0.0, dN3deta, 0.0, dN4deta, 0.0, dN5deta, 0.0, dN6deta, 0.0, dN7deta, 0.0, dN8deta}};
		}
	}
}
