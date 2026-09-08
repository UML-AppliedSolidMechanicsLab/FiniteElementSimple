/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/22/2019
 * Time: 1:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace FiniteElementSimple.Materials
{
	/// <summary>
	/// Description of LinElastic1D.
	/// </summary>
	public class LinElastic1D:Material
	{
		private double modulus;
		
		public LinElastic1D(double E)
		{
			modulus = E;
		}
		public override double [,] D(double xi, double eta, double zeta){
			
			return new double[1,1] {{modulus}};
		}
	}
	
	/// <summary>
	/// Description of LinElastic2DPlaneStress.
	/// </summary>
	public class LinElastic2DPlaneStress:Material
	{
		private double E;
		private double nu;
		private double C;
		
		public LinElastic2DPlaneStress(double E, double nu)
		{
			this.E = E;
			this.nu = nu;
			C = E / (1.0 - nu * nu);
		}
		public override double [,] D(double xi, double eta, double zeta){
			
			return new double[3,3] {{C*1.0, C*nu, 0.0}, 
				{C*nu, C*1.0, 0.0}, 
				{0.0, 0.0, C*0.5*(1.0-nu)}};
		}
	}
}
