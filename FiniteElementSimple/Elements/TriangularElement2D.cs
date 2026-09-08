using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomMath;
using FiniteElementSimple.Materials;
using FiniteElementSimple.BoundaryConditions;

namespace FiniteElementSimple.Elements
{

	public struct IntPt2D
	{
		public double X, Y;
		public IntPt2D(double x, double y)
		{
			X = x;
			Y = y;
		}
	}

	public abstract class TriangularElement2D : Element2D
	{

		#region private members

		//Weights for Quadrature
		protected double[][] w;
		//integration Pts for Quadrature
		protected IntPt2D[][] loc;

		protected int nIntPts;
		#endregion

		#region Constructor
		public TriangularElement2D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
									 double thickness, double[][] nodalLocations, int n_gaussQuadPoints)
			: base(elementMaterial, localToGlobalConnectivity_1index, thickness, nodalLocations)
		{
			//Just hard-wired stuff for gaussian quadrature (integrating from 0 to 1 in xi and 0 to 1-xi in eta)
			//From http://me.rice.edu/~akin/Elsevier/Chap_10.pdf
			w = new double[4][];
			w[0] = new double[] { 0.5 };
			w[1] = new double[] { 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0 };
			w[2] = new double[] { -9d / 32d, 25d/ 96d, 25d / 96d, 25d / 96d };
			w[3] = new double[] { 1d/ 40d, 1d/ 15d, 1d/ 40d, 1d/ 15d, 1d/ 40d, 1d/ 15d, 9d/ 40d};

			loc = new IntPt2D[4][];
			loc[0] = new IntPt2D[] { new IntPt2D( 1.0 / 3.0, 1.0 / 3.0 ) };
			loc[1] = new IntPt2D[] { new IntPt2D(1d/6d, 1d/6d), new IntPt2D(2d/3d, 1d/6d), new IntPt2D(1d/6d, 2d/3d) };
			loc[2] = new IntPt2D[] { new IntPt2D(1d/3d, 1d/3d), new IntPt2D(3d/5d, 1d/5d), new IntPt2D(1d/5d, 3d/5d), 
				new IntPt2D(1d/5d, 1d/5d) };
			loc[3] = new IntPt2D[] { new IntPt2D(0d,0d), new IntPt2D(.5,0d), new IntPt2D(1d,0d), new IntPt2D(.5,.5),
				new IntPt2D(0d,1d), new IntPt2D(0d, 0.5), new IntPt2D(1d/3d, 1d/3d)};

			nIntPts = n_gaussQuadPoints;
		}
		#endregion

		#region public methods
		public override double Det_Of_J(double xi, double eta, double zeta)
		{
			//Slide the xi in there because the indegral of the area is r*dr*dtheta
			double[,] myJ = J(xi, eta, zeta);
			return MatrixMath.Determinant(myJ) * xi;
		}

		public override void IntegrateKandFOverVolume()
		{
			double zeta = 0.0;
			for (int i = 0; i < w[nIntPts].Length; i++)
			{
				double xi = loc[nIntPts][i].X;
				double eta = loc[nIntPts][i].Y;
				
					//Get some of the initial matrices at the gaussian locations
					double[,] Btemp = B(xi, eta, zeta);
					double[,] Dtemp = elementMaterial.D(xi, eta, zeta);
					double[,] NTtemp = MatrixMath.Transpose(ShapeFunction(xi, eta, zeta));
					double det_J = Det_Of_J(xi, eta, zeta) * (thickness);

					double[,] BTD = MatrixMath.Multiply(MatrixMath.Transpose(Btemp), Dtemp);
					double[,] BTDBJ = MatrixMath.ScalarMultiply( det_J * w[nIntPts][i], MatrixMath.Multiply(BTD, Btemp));
					k = MatrixMath.Add(BTDBJ, k);

					foreach (BodyForce bf in lBodyForces)
					{
						double[] f_b = bf.BodyForce_per_Area(xi, eta, zeta);
						double[] NTf_body = MatrixMath.Multiply(NTtemp, f_b);
						double[] tempF = VectorMath.ScalarMultiply((thickness) * det_J * w[nIntPts][i], NTf_body);
						f = VectorMath.Add(tempF, f);
					}
					foreach (InitialStrain el in lInitialStrain)
					{
						double[] x = GlobalXPosition(xi, eta, zeta);
						double[] e0 = el.epsilon_0(x[0], x[1], x[2]);
						double[] NTe_0 = MatrixMath.Multiply(BTD, e0);
						double[] tempe0 = VectorMath.ScalarMultiply((thickness) * det_J * w[nIntPts][i], NTe_0);
						f = VectorMath.Add(tempe0, f);

					}
			}
			foreach (SurfaceTraction st in lSurfaceTractions)
			{
				f = VectorMath.Add(f, st.F_fromSurfaceTraction(this));
			}
		}

		public override void DrawOutline(out double[] X, out double[] Y, int nPtsPerSide)
		{
			X = new double[3* nPtsPerSide + 1];
			Y = new double[3 * nPtsPerSide + 1];
			int count = 0;
			double[] tempLocation;
            //Bottom Surface (xi = 0 .. 1, eta = 0)
            for (int i = 0; i < nPtsPerSide; i++)
            {
				tempLocation = GlobalXPosition(1d * i / nPtsPerSide, 0d, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
            }
			//angle (xi = 1..0, eta = 1-xi)
			for (int i = nPtsPerSide; i > 0; i--)
			{
				tempLocation = GlobalXPosition(1d * i / nPtsPerSide, 1d - 1d * i / nPtsPerSide, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//down the side (xi = 0, eta = 1 .. 0)
			for (int i = nPtsPerSide; i > 0; i--)
			{
				tempLocation = GlobalXPosition(0d, 1d * i / nPtsPerSide, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//Now reconnect:
			X[count] = X[0];
			Y[count] = Y[0];
		}
		#endregion


	}
}

