/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/22/2019
 * Time: 1:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RandomMath;
using System.Collections.Generic;
using FiniteElementSimple.Materials;
using FiniteElementSimple.BoundaryConditions;

namespace FiniteElementSimple.Elements
{
	/// <summary>
	/// Description of Element.
	/// </summary>
	public abstract class Element
	{
		#region Properties
		protected Material elementMaterial;
		protected double [] q;
		protected double [] f;
		protected double [,] k;

		public int [] localToGlobalConnectivity;
		public int nDOFperNode;
		public List<SurfaceTraction> lSurfaceTractions = new List<SurfaceTraction>();
		public List<BodyForce> lBodyForces = new List<BodyForce>();
		public List<InitialStrain> lInitialStrain = new List<InitialStrain>();

        #endregion

        #region Public Properties
		public virtual double[] Q { get { return q; } set { q = value; } }
		public virtual double[] F { get { return f; } set { f = value; } }
		public virtual double[,] K { get { return k; }  set { k = value; } }

        #endregion

        #region Constructor
        protected Element(Material elementMaterial, int [] localToGlobalConnectivity_1index, int nDOFperNode)
		{
			this.elementMaterial = elementMaterial;
			this.nDOFperNode = nDOFperNode;
			this.localToGlobalConnectivity = localToGlobalConnectivity_1index;

			//Initiate k and f
			k = new double[localToGlobalConnectivity.Length * nDOFperNode, localToGlobalConnectivity.Length * nDOFperNode];
			f = new double[localToGlobalConnectivity.Length * nDOFperNode];
			q = new double[localToGlobalConnectivity.Length * nDOFperNode];
		}
		#endregion

		#region Public Methods

		public virtual void DrawOutline(out double[] X, out double[] Y, int nPtsPerSide)
        {
			X = new double[localToGlobalConnectivity.Length + 1];
			Y = new double[localToGlobalConnectivity.Length + 1];
		}
		public abstract double [,] ShapeFunction(double xi, double eta, double zeta);
		
		public abstract double [,] B(double xi, double eta, double zeta);
		
		public virtual double Det_Of_J(double xi, double eta, double zeta){
			double [,] myJ = J(xi, eta, zeta);
			return MatrixMath.Determinant(myJ);
		}
		
		public abstract double [,] J(double xi, double eta, double zeta);
		
		public abstract double [] GlobalXPosition(double xi, double eta, double zeta);

		public abstract void IntegrateKandFOverVolume();
		
		public double [] Displacement(double xi, double eta, double zeta){
			return MatrixMath.Multiply(ShapeFunction(xi, eta, zeta), q);
			
		}
		
		public double [] Strain(double xi, double eta, double zeta){
			return MatrixMath.Multiply(B(xi, eta, zeta), q);
		}
		
		public double [] Stress(double xi, double eta, double zeta){
			double [] tempStrain = Strain(xi, eta, zeta);
			
			foreach (InitialStrain e0 in lInitialStrain) {
				double [] globalx = GlobalXPosition(xi, eta, zeta);
				tempStrain = VectorMath.Subtract(tempStrain, e0.epsilon_0(globalx[0], globalx[1], globalx[2]));
				}
			return MatrixMath.Multiply(elementMaterial.D(xi, eta, zeta), tempStrain);
		}
		
		public void AssignNodelQToLocalq(int nodeNumber, double [] nodalQ)
        {
			VectorMath.CopyToVector(ref q, nodalQ, nodeNumber * nDOFperNode);
        }
		#endregion

		#region private methods

		#endregion

		#region This code is for 3-D Gauss Quad someday:
		/*
		for (int i = 0; i < nIntPts_xi; i++) {
				double xi = loc[nIntPts_xi-1][i];
				
				for (int j = 0; j < nIntPts_eta; j++) {
					double eta = loc[nIntPts_eta-1][j];
					
					for (int l = 0; l < nIntPts_zeta; l++) {
						double zeta = loc[nIntPts_zeta-1][l];
						
						//Get some of the initial matrices at the gaussian locations
						double [,] Btemp = B(xi, eta, zeta);
						double [,] Dtemp = elementMaterial.D(xi, eta, zeta);
						double [,] NTtemp = MatrixMath.Transpose(ShapeFunction(xi, eta, zeta));
						double det_J = Det_Of_J(xi, eta, zeta);
						
						double [,] BTD = MatrixMath.Multiply(MatrixMath.Transpose(Btemp), Dtemp);
						double [,] BTDBJ = MatrixMath.ScalarMultiply(det_J * w[nIntPts_xi-1][i] * w[nIntPts_eta-1][j] * w[nIntPts_zeta-1][l],
						                                             MatrixMath.Multiply( BTD, Btemp));
						k = MatrixMath.Add(BTDBJ, k);
						
						foreach (BodyForce bf in lBodyForces) {
							double [] f_b = bf.BodyForce_per_Area(xi, eta, zeta);
							double [] NTf_body = MatrixMath.Multiply(NTtemp, f_b);
							double [] tempF = VectorMath.ScalarMultiply( det_J * w[nIntPts_xi-1][i] * w[nIntPts_eta-1][j] * w[nIntPts_zeta-1][l],
							                                            NTf_body);
							f = VectorMath.Add(tempF, f);
						}
						foreach (InitialStrain el in lInitialStrain) {
							double [] x = GlobalXPosition(xi, eta, zeta);
							double [] e0 = el.epsilon_0(x[0], x[1], x[2]);
							double [] NTe_0 = MatrixMath.Multiply(BTD, e0);
							double [] tempe0 = VectorMath.ScalarMultiply( det_J * w[nIntPts_xi-1][i] * w[nIntPts_eta-1][j] * w[nIntPts_zeta-1][l],
							                                             NTe_0);
							f = VectorMath.Add(tempe0, f);
							
						}
					}
				}
			}
			foreach (SurfaceTraction st in lSurfaceTractions) {
				f = VectorMath.Add(f, st.F_fromSurfaceTraction(this));
			}
		 */
		#endregion
	}

}
