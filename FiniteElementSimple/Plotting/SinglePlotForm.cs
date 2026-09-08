/*
 * Replacement for the ZedGraph-backed SinglePlotZedGraph.SinglePlotForm, implemented on top of ScottPlot.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ScottPlot.WinForms;

namespace FiniteElementSimple.Plotting
{
    /// <summary>
    /// A simple WinForms window hosting a single ScottPlot plot area.
    /// Provides the same two constructor shapes that were previously provided by SinglePlotZedGraph.SinglePlotForm
    /// so call sites only need a using/namespace change.
    /// </summary>
    public class SinglePlotForm : Form
    {
        private readonly FormsPlot formsPlot;

        /// <summary>
        /// Plots one or more X/Y curves as connected lines. Used for mesh outlines and result-vs-x plots.
        /// </summary>
        public SinglePlotForm(string title, string xLabel, string yLabel, List<string> lLabels, List<double[]> lX, List<double[]> lY,
                               bool showLegend = true, bool showMarkers = true)
        {
            formsPlot = CreateFormsPlot(title);

            formsPlot.Plot.Title(title);
            formsPlot.Plot.XLabel(xLabel);
            formsPlot.Plot.YLabel(yLabel);

            for (int i = 0; i < lX.Count; i++) {
                var scatter = formsPlot.Plot.Add.Scatter(lX[i], lY[i]);
                scatter.LegendText = i < lLabels.Count ? lLabels[i] : string.Empty;
                if (!showMarkers) {
                    scatter.MarkerSize = 0;
                }
            }

            formsPlot.Plot.Legend.IsVisible = showLegend;
            formsPlot.Refresh();
        }

        /// <summary>
        /// Plots a cloud of X/Y points colored by a Z value, binned into the supplied discrete color scheme.
        /// Used for contour-style plots (displacement/strain/stress/jacobian over an element).
        /// </summary>
        public SinglePlotForm(string title, double[] xData, double[] yData, double[] zData, Color[] colorScheme)
        {
            formsPlot = CreateFormsPlot(title);

            formsPlot.Plot.Title(title);
            formsPlot.Plot.XLabel("x");
            formsPlot.Plot.YLabel("y");

            double zMin = double.MaxValue, zMax = double.MinValue;
            foreach (double z in zData) {
                if (z < zMin) zMin = z;
                if (z > zMax) zMax = z;
            }
            double range = (zMax - zMin == 0) ? 1 : zMax - zMin;
            int nBins = colorScheme.Length;

            var binX = new List<double>[nBins];
            var binY = new List<double>[nBins];
            for (int b = 0; b < nBins; b++) {
                binX[b] = new List<double>();
                binY[b] = new List<double>();
            }

            for (int i = 0; i < zData.Length; i++) {
                double t = (zData[i] - zMin) / range;
                int bin = Math.Min(nBins - 1, (int)(t * nBins));
                binX[bin].Add(xData[i]);
                binY[bin].Add(yData[i]);
            }

            for (int b = 0; b < nBins; b++) {
                if (binX[b].Count == 0) continue;
                var scatter = formsPlot.Plot.Add.Scatter(binX[b].ToArray(), binY[b].ToArray());
                scatter.LineWidth = 0;
                scatter.MarkerSize = 6;
                scatter.Color = new ScottPlot.Color(colorScheme[b].R, colorScheme[b].G, colorScheme[b].B);
                scatter.LegendText = string.Format("{0:0.###} to {1:0.###}", zMin + b * range / nBins, zMin + (b + 1) * range / nBins);
            }

            formsPlot.Plot.Legend.IsVisible = true;
            formsPlot.Refresh();
        }

        /// <summary>
        /// Refreshes the plot render. Kept for compatibility with the previous ZedGraph-based API.
        /// </summary>
        public void Plot()
        {
            formsPlot.Refresh();
        }

        private FormsPlot CreateFormsPlot(string title)
        {
            Text = title;
            Width = 800;
            Height = 600;
            var fp = new FormsPlot { Dock = DockStyle.Fill };
            Controls.Add(fp);
            return fp;
        }
    }
}
