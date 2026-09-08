/*
 * Created by SharpDevelop.
 * User: scott_stapleton
 * Date: 2/25/2019
 * Time: 11:49 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple
{
	/// <summary>
	/// Description of Assembly.
	/// </summary>
	public class Assembly
	{
		#region Private Members
		private double C; //Large Constant involved in the Penalty Approach when applying BC's
		private int nDOFTot;
		#endregion
		
		#region Public Members
		public double CurrentC{
			get{return C;}
		}
		public List<Element> lElements;
		public List<BC> lLoads;
		public List<BC> lBCs;
		public double [,] GlobalK;
		public double [] GlobalF;
		public double [] GlobalQ;
		public int nDOFperNode;
		
		#endregion
		
		#region Constructors
		
		///This constructor is used to make a new Assembly
		public Assembly(List<Element> lElements, List<BC> lLoads, List<BC> lBCs, int nDOFperNode)
		{
			this.lBCs = lBCs;
			this.lLoads = lLoads;
			this.lElements = lElements;
			this.nDOFperNode = nDOFperNode;
			nDOFTot = FindMaxDOF();
			
			//instantiate K and F:
			GlobalF = new double[nDOFTot];
			GlobalK = new double[nDOFTot, nDOFTot];
		}
		
		#endregion
		
		#region Public Methods
		
		public void Solve(){
			
			AssembleLocalKandF();
			ApplyLoads();
			ApplyDisplacementBCs();
			//Actually solve
			GlobalQ = RandomMath.MatrixMath.LinSolve(GlobalK, GlobalF);
			AssignGlobalQToElements();
			
		}

		#endregion
		
		#region Private Methods
		private void AssignGlobalQToElements(){
			
			//Now add the local K from each element
			foreach (Element el in lElements) {
				
				//loop through the local dofs
				
				for (int i = 0; i < el.localToGlobalConnectivity.Length; i++) {
					for (int k = 0; k < nDOFperNode; k++) {
						
						el.Q[i*el.nDOFperNode + k] = GlobalQ[(el.localToGlobalConnectivity[i]-1) * nDOFperNode + k];
					}
				}
				
				
			}
		}
		
		public void AssembleLocalKandF(){
			
			//Now add the local K from each element
			foreach (Element el in lElements) {
				
				el.IntegrateKandFOverVolume();
				
				//loop through the local dofs
				//HACK This assumes that the local nDOFperNode for each element is the same as the global.  BOOOOO!
				for (int i = 0; i < el.localToGlobalConnectivity.Length; i++) {
					for (int k = 0; k < nDOFperNode; k++) {
						for (int j = 0; j < el.localToGlobalConnectivity.Length; j++) {
							for (int l = 0; l < nDOFperNode; l++) {
								int i1 = i* nDOFperNode + k;
								int j1 = j * nDOFperNode + l;
								double tempK = el.K[i1, j1];
								int i2 = (el.localToGlobalConnectivity[i]-1) * nDOFperNode + k;
								int j2 = (el.localToGlobalConnectivity[j]-1) * nDOFperNode + l;
								GlobalK[i2,j2] += tempK;
							}
						}
						GlobalF[(el.localToGlobalConnectivity[i]-1)* nDOFperNode + k] += el.F[i* nDOFperNode + k];
					}
				}
			}
		}
		
		private int FindMaxDOF(){
			
			int maxNodeNumber = 0;
			foreach (Element el in lElements) {
				int maxElementNodeNumber = el.localToGlobalConnectivity.Max();
				if (maxElementNodeNumber > maxNodeNumber) {
					maxNodeNumber = maxElementNodeNumber;
				}
			}
			
			maxNodeNumber ++;
			
			return (maxNodeNumber - 1) * nDOFperNode;
		}
		
		private void ApplyLoads()
		{
			foreach (BC fbc in lLoads) {
				GlobalF[fbc.dofNumber] += fbc.magnitude;
			}
		}
		
		private void ApplyDisplacementBCs()
		{
			C = RandomMath.MatrixMath.GetMax(GlobalK)*1e4;
			
			foreach (BC dbc in lBCs) {
				GlobalF[dbc.dofNumber] += dbc.magnitude * C;
				GlobalK[dbc.dofNumber, dbc.dofNumber] += C;
			}
		}
		
		#endregion
	}
	public class BC
	{
		public int dofNumber;
		public double magnitude;
		
		public BC(int dofNumber, double magnitude){
			this.dofNumber = dofNumber;
			this.magnitude = magnitude;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="nodeNumber">Node number is based on a 1-index system</param>
		/// <param name="dofNumberInNode"></param>
		/// <param name="nDOFperNode"></param>
		/// <param name="magnitude"></param>
		public BC(int nodeNumber, int dofNumberInNode, int nDOFperNode, double magnitude){
			this.dofNumber =( nodeNumber-1) * nDOFperNode + dofNumberInNode;
			this.magnitude = magnitude;
		}
	}
}
