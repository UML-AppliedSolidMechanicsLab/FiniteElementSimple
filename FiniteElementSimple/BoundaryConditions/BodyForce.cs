/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/28/2019
 * Time: 1:51 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace FiniteElementSimple.BoundaryConditions
{
	/// <summary>
	/// Description of BodyForce: this assumes a constant body force. 
	/// </summary>
	public class BodyForce
	{
		private double [] bodyForceMagnitude;
			
		public BodyForce(double [] bodyForceMagnitude)
		{
			this.bodyForceMagnitude = bodyForceMagnitude;
		}
		
		public double [] BodyForce_per_Area(double xi, double eta, double zeta){
			
			return bodyForceMagnitude;
		}
	}
}
