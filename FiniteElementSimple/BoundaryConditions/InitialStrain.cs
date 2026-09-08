/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 3/21/2019
 * Time: 10:51 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace FiniteElementSimple.BoundaryConditions
{
	/// <summary>
	/// Description of InitialStrain.
	/// </summary>
	public class InitialStrainDt_1D:InitialStrain
	{
		double [] dT;
		double alpha;
		
		public InitialStrainDt_1D( double [] dt, double alpha):base()
		{
			this.dT = dt;
			this.alpha = alpha;
		}
		
		public override double[] epsilon_0(double x, double y, double z){
			
			double Tsum = 0.0;
			for (int i = 0; i < dT.Length; i++) {
				Tsum += dT[i] * Math.Pow(x,(double)i);
			}
			
			return new double[] {alpha * Tsum};
		}
		
	}
	
	public abstract class InitialStrain
	{
		protected InitialStrain(){}
		
		public abstract double[] epsilon_0(double x, double y, double z);
		
	}
}
