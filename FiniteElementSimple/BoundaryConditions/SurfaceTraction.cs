/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/28/2019
 * Time: 12:55 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RandomMath;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple.BoundaryConditions
{
	/// <summary>
	/// Description of SurfaceForce.
	/// </summary>
	public abstract class SurfaceTraction
	{
		protected SurfaceTraction(){}
		
		public abstract double [] F_fromSurfaceTraction(Element currentElement);
		
	}
	
	/// <summary>
	/// Description of SurfaceTraction1D.
	/// </summary>
	public class SurfaceTraction1D:SurfaceTraction
	{
		
		private double [] surfaceTractionCoefficients;
		private int nGaussPts;
		private double otherDimension;
		
		public SurfaceTraction1D(double [] surfaceTractionCoefficients, double otherDimension, int nGaussPts):base(){
			
			this.surfaceTractionCoefficients = surfaceTractionCoefficients;
			this.nGaussPts = nGaussPts;
			this.otherDimension = otherDimension;
		}
		
		private double [] Surface_Traction(double globalX){
			double st = 0.0;
			for (int i = 0; i < surfaceTractionCoefficients.Length; i++) {
				st += surfaceTractionCoefficients[i] * Math.Pow(globalX, i);
			}
			return new double[]{st};
		}
		
		public override double [] F_fromSurfaceTraction(Element currentElement){
			
			//Initiate k and f
			double [] f = new double[currentElement.localToGlobalConnectivity.Length];
			
			//Just hard-wired stuff...
			//TODO: put this into a general method...
			double [][] w = new double[4][];
			w[0] = new double[]{2.0};
			w[1] = new double[]{1.0, 1.0};
			w[2] = new double[]{0.555556, 0.555556, 0.8888889};
			w[3] = new double[]{0.3478548451, 0.3478548451, 0.6521451549, 0.6521451549};
			
			double [][] loc = new double[4][];
			loc[0] = new double[]{0.0};
			loc[1] = new double[]{0.5773502692, -0.5773502692};
			loc[2] = new double[]{0.77459666692, -0.77459666692, 0.0};
			loc[3] = new double[]{0.8611363116, -0.8611363116, 0.3399810436, -0.3399810436};
			
			for (int i = 0; i < nGaussPts; i++) {
				double xi = loc[nGaussPts-1][i];
				
				//Get some of the initial matrices at the gaussian locations
				double [] xGlobal = currentElement.GlobalXPosition(xi, 0.0, 0.0);
				double [,] NTtemp = MatrixMath.Transpose(currentElement.ShapeFunction(xi, 0.0, 0.0));
				double [] f_s = Surface_Traction(xGlobal[0]);
				
				double [] NTf_s = MatrixMath.Multiply(NTtemp, f_s);
				double [] tempF = VectorMath.ScalarMultiply( otherDimension * MatrixMath.Determinant(currentElement.J(xi,0.0,0.0)) * w[nGaussPts-1][i], NTf_s);
				//double [] tempF = VectorMath.ScalarMultiply( otherDimension * (currentElement.Det_Of_J(xi,0.0,0.0)) * w[nGaussPts-1][i], NTf_s);
				
				f = VectorMath.Add(tempF, f);
			}
			return f;
		}
	}
}
