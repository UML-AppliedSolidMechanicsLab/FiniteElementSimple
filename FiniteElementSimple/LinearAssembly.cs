/*
 * LinearAssembly: the existing linear finite-element solve, extracted from Assembly so that
 * Assembly can serve as a common base class for future solution strategies.
 */
using System.Collections.Generic;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple
{
    /// <summary>
    /// Finite-element assembly using the existing linear solution sequence: assemble global K/F,
    /// apply loads, apply displacement BCs (penalty approach), solve GlobalK * GlobalQ = GlobalF,
    /// and assign the resulting global displacements back to each element.
    /// </summary>
    public class LinearAssembly : Assembly
    {
        ///This constructor is used to make a new LinearAssembly
        public LinearAssembly(List<Element> lElements, List<BC> lLoads, List<BC> lBCs, int nDOFperNode)
            : base(lElements, lLoads, lBCs, nDOFperNode)
        {
        }

        public override void Solve(){

            AssembleLocalKandF();
            ApplyLoads();
            ApplyDisplacementBCs();
            //Actually solve
            GlobalQ = RandomMath.MatrixMath.LinSolve(GlobalK, GlobalF);
            AssignGlobalQToElements();

        }
    }
}
