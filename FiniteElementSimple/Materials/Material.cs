/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/22/2019
 * Time: 1:58 PM
 */
using System;

namespace FiniteElementSimple.Materials
{
	/// <summary>
	/// Description of Material.
	/// </summary>
	public abstract class Material
	{
		protected Material()
		{
		}
		public abstract double [,] D(double xi, double eta, double zeta);
	}
}
