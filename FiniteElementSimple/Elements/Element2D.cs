using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomMath;
using FiniteElementSimple.Materials;

namespace FiniteElementSimple.Elements
{
    public abstract class Element2D : Element
	{

		#region Properties
		protected double[] nodalLocations;
		protected double[,] A = {{1.0, 0.0, 0.0, 0.0},
			{0.0, 0.0, 0.0, 1.0},
			{0.0, 1.0, 1.0, 0.0}};
		protected double thickness;
        #endregion

        public Element2D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
									 double thickness, double[][] nodalLocations)
			: base(elementMaterial, localToGlobalConnectivity_1index, 2)
		{
			this.thickness = thickness;

			//convert the nodal locations into a column vector, in the form of x1, y1, x2, y2, .... x8, y8
			this.nodalLocations = new double[nodalLocations.Length * nodalLocations[0].Length];
			int count = 0;
			for (int i = 0; i < nodalLocations.Length; i++)
			{
				for (int j = 0; j < nodalLocations[i].Length; j++)
				{
					this.nodalLocations[count] = nodalLocations[i][j];
					count++;
				}
			}
		}

		public abstract double[,] DNdxi(double xi, double eta, double zeta);

		public override double[,] B(double xi, double eta, double zeta)
		{

			double[,] dNhat = DNdxi(xi, eta, zeta);
			double[,] Jinv = MatrixMath.InvertMatrix(J(xi, eta, zeta));

			double[,] Jinvhat = new double[4, 4];
			MatrixMath.CopyToMatrix(ref Jinvhat, Jinv, 0, 0);
			MatrixMath.CopyToMatrix(ref Jinvhat, Jinv, 2, 2);

			double[,] AJinvhat = MatrixMath.Multiply(A, Jinvhat);

			return MatrixMath.Multiply(AJinvhat, dNhat);
		}

		public override double[,] J(double xi, double eta, double zeta)
		{

			double[] Jhat = MatrixMath.Multiply(DNdxi(xi, eta, zeta), nodalLocations);

			return new double[,] { { Jhat[0], Jhat[2] }, { Jhat[1], Jhat[3] } };
		}

		public override double[] GlobalXPosition(double xi, double eta, double zeta)
		{
			double[,] N = ShapeFunction(xi, eta, zeta);
			return MatrixMath.Multiply(N, nodalLocations);
		}
	}
}
