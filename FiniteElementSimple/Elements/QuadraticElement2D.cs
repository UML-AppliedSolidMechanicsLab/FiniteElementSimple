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

	public abstract class QuadraticElement2D: Element2D
    {

        #region private members

        //Weights for Quadrature
        protected double[][] w;
        //integration Pts for Quadrature
        protected double[][] loc;

        protected int nIntPts_xi;
        protected int nIntPts_eta;
        #endregion

        #region Constructor
        public QuadraticElement2D(Material elementMaterial, int[] localToGlobalConnectivity_1index,
                                     double thickness, double[][] nodalLocations,
                                     int n_gaussQuadPoints_xi, int n_gaussQuadPoints_eta)
            : base(elementMaterial, localToGlobalConnectivity_1index, thickness, nodalLocations)
        {
            //Just hard-wired stuff for gaussian quadrature (integrating from -1 to 1 in xi and eta
            w = new double[4][];
            w[0] = new double[] { 2.0 };
            w[1] = new double[] { 1.0, 1.0 };
            w[2] = new double[] { 0.555556, 0.555556, 0.8888889 };
            w[3] = new double[] { 0.3478548451, 0.3478548451, 0.6521451549, 0.6521451549 };

            loc = new double[4][];
            loc[0] = new double[] { 0.0};
            loc[1] = new double[] { 0.5773502692, -0.5773502692 };
            loc[2] = new double[] { 0.77459666692, -0.77459666692, 0.0 };
            loc[3] = new double[] { 0.8611363116, -0.8611363116, 0.3399810436, -0.3399810436 };

            this.nIntPts_xi = n_gaussQuadPoints_xi;
            this.nIntPts_eta = n_gaussQuadPoints_eta;
        }
        #endregion

        #region public methods

        public override void IntegrateKandFOverVolume()
        {
			double zeta = 0.0;
			for (int i = 0; i < nIntPts_xi; i++)
			{
				double xi = loc[nIntPts_xi - 1][i];

				for (int j = 0; j < nIntPts_eta; j++)
				{
					double eta = loc[nIntPts_eta - 1][j];

						//Get some of the initial matrices at the gaussian locations
						double[,] Btemp = B(xi, eta, zeta);
						double[,] Dtemp = elementMaterial.D(xi, eta, zeta);
						double[,] NTtemp = MatrixMath.Transpose(ShapeFunction(xi, eta, zeta));
						double det_J = Det_Of_J(xi, eta, zeta) * thickness;

						double[,] BTD = MatrixMath.Multiply(MatrixMath.Transpose(Btemp), Dtemp);
						double[,] BTDBJ = MatrixMath.ScalarMultiply(det_J * w[nIntPts_xi - 1][i] * w[nIntPts_eta - 1][j],
																	 MatrixMath.Multiply(BTD, Btemp));
						k = MatrixMath.Add(BTDBJ, k);

						foreach (BodyForce bf in lBodyForces)
						{
							double[] f_b = bf.BodyForce_per_Area(xi, eta, zeta);
							double[] NTf_body = MatrixMath.Multiply(NTtemp, f_b);
							double[] tempF = VectorMath.ScalarMultiply(det_J * w[nIntPts_xi - 1][i] * w[nIntPts_eta - 1][j], NTf_body);
							f = VectorMath.Add(tempF, f);
						}
						foreach (InitialStrain el in lInitialStrain)
						{
							double[] x = GlobalXPosition(xi, eta, zeta);
							double[] e0 = el.epsilon_0(x[0], x[1], x[2]);
							double[] NTe_0 = MatrixMath.Multiply(BTD, e0);
							double[] tempe0 = VectorMath.ScalarMultiply(det_J * w[nIntPts_xi - 1][i] * w[nIntPts_eta - 1][j],  NTe_0);
							f = VectorMath.Add(tempe0, f);

						}
				}
			}
			foreach (SurfaceTraction st in lSurfaceTractions)
			{
				f = VectorMath.Add(f, st.F_fromSurfaceTraction(this));
			}
		}

		public override void DrawOutline(out double[] X, out double[] Y, int nPtsPerSide)
		{
			X = new double[4 * nPtsPerSide + 1];
			Y = new double[4 * nPtsPerSide + 1];
			int count = 0;
			double[] tempLocation;
			//Bottom Surface (xi = 0 .. 1, eta = 0)
			for (int i = 0; i < nPtsPerSide; i++)
			{
				tempLocation = GlobalXPosition(-1d + 2d * i / nPtsPerSide, -1d, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//right (xi = 1..0, eta = 1-xi)
			for (int i = 0; i < nPtsPerSide; i++)
			{
				tempLocation = GlobalXPosition(1d, -1d + 2d * i / nPtsPerSide, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//Top
			for (int i = nPtsPerSide; i > 0; i--)
			{
				tempLocation = GlobalXPosition(-1d + 2d * i / nPtsPerSide, 1d, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//Left
			for (int i = nPtsPerSide; i > 0; i--)
			{
				tempLocation = GlobalXPosition(-1d, -1d + 2d * i / nPtsPerSide, 0d);
				X[count] = tempLocation[0];
				Y[count] = tempLocation[1];
				count++;
			}
			//Now reconnect:
			X[count] = X[0];
			Y[count] = Y[1];
		}
		#endregion


	}
}
