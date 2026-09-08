/*
 * Moved out of Assembly.cs so that the finite-element Assembly class has no dependency on visualization.
 */
using System.Collections.Generic;
using FiniteElementSimple.Elements;

namespace FiniteElementSimple.Plotting
{
    /// <summary>
    /// Provides mesh-outline plotting for an Assembly. Kept separate from the finite-element
    /// Assembly class so that Assembly has no dependency on plotting/visualization.
    /// </summary>
    public static class MeshPlotter
    {
        public static void PlotOutline(Assembly assembly, int nPointsPerSide)
        {
            //plot a little x/y axis
            List<double[]> lX = new List<double[]>();
            List<double[]> lY = new List<double[]>();
            List<string> lLabels = new List<string>();
            //Loop through each element
            for (int i = 0; i < assembly.lElements.Count; i++)
            {
                assembly.lElements[i].DrawOutline(out double[] X, out double[] Y, nPointsPerSide);
                lX.Add(X);
                lY.Add(Y);
                lLabels.Add(i.ToString());
            }

            SinglePlotForm myPlot = new SinglePlotForm("Mesh", "x", "y", lLabels, lX, lY, showLegend: false, showMarkers: false);
            myPlot.Plot();
            //myPlot.Activate();
            //myPlot.ShowDialog();
        }
    }
}
