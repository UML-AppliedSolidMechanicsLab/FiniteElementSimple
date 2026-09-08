
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
	public abstract class Element1D : Element
	{

		#region Properties

		//Weights for Quadrature
		protected double[][] w;
		//integration Pts for Quadrature
		protected double[][] loc;

		protected int nIntPts;

		protected double[] nodalLocations;
		protected double[,] A = {{1.0}};
		protected double area;
		#endregion

		public Element1D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
									 double area, int n_gaussQuadPoints)
			: base(elementMaterial, localToGlobalConnectivity_1index, 1)
		{
			this.area = area;
			nIntPts = n_gaussQuadPoints;

			//Just hard-wired stuff for gaussian quadrature (integrating from -1 to 1 in xi and eta
			w = new double[4][];
			w[0] = new double[] { 2.0 };
			w[1] = new double[] { 1.0, 1.0 };
			w[2] = new double[] { 0.555556, 0.555556, 0.8888889 };
			w[3] = new double[] { 0.3478548451, 0.3478548451, 0.6521451549, 0.6521451549 };

			loc = new double[4][];
			loc[0] = new double[] { 0.0 };
			loc[1] = new double[] { 0.5773502692, -0.5773502692 };
			loc[2] = new double[] { 0.77459666692, -0.77459666692, 0.0 };
			loc[3] = new double[] { 0.8611363116, -0.8611363116, 0.3399810436, -0.3399810436 };

		}

		public override void IntegrateKandFOverVolume()
		{
			double zeta = 0.0;
			double eta = 0.0;
			for (int i = 0; i < nIntPts; i++)
			{
				double xi = loc[nIntPts - 1][i];

				

					//Get some of the initial matrices at the gaussian locations
					double[,] Btemp = B(xi, eta, zeta);
					double[,] Dtemp = elementMaterial.D(xi, eta, zeta);
					double[,] NTtemp = MatrixMath.Transpose(ShapeFunction(xi, eta, zeta));
					double det_J = Det_Of_J(xi, eta, zeta) * (area);

					double[,] BTD = MatrixMath.Multiply(MatrixMath.Transpose(Btemp), Dtemp);
					double[,] BTDBJ = MatrixMath.ScalarMultiply(det_J * w[nIntPts - 1][i],MatrixMath.Multiply(BTD, Btemp));
					k = MatrixMath.Add(BTDBJ, k);

					foreach (BodyForce bf in lBodyForces)
					{
						double[] f_b = bf.BodyForce_per_Area(xi, eta, zeta);
						double[] NTf_body = MatrixMath.Multiply(NTtemp, f_b);
						double[] tempF = VectorMath.ScalarMultiply(det_J * w[nIntPts - 1][i], NTf_body);
						f = VectorMath.Add(tempF, f);
					}
					foreach (InitialStrain el in lInitialStrain)
					{
						double[] x = GlobalXPosition(xi, eta, zeta);
						double[] e0 = el.epsilon_0(x[0], x[1], x[2]);
						double[] NTe_0 = MatrixMath.Multiply(BTD, e0);
						double[] tempe0 = VectorMath.ScalarMultiply(det_J * w[nIntPts - 1][i], NTe_0);
						f = VectorMath.Add(tempe0, f);

					}
			}
			foreach (SurfaceTraction st in lSurfaceTractions)
			{
				f = VectorMath.Add(f, st.F_fromSurfaceTraction(this));
			}
		}

	}
}

