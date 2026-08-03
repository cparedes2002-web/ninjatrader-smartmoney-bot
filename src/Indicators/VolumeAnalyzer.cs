#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Volume Analyzer Indicator
    /// Identifies institutional volume patterns and anomalies
    /// Key component of Smart Money Concept detection
    /// 
    /// Usage: Confirms entry signals when volume exceeds thresholds
    /// </summary>
    public class VolumeAnalyzer : Indicator
    {
        #region Variables
        private int lookbackPeriod = 20;
        private double volumeThreshold = 1.5;
        private double[] volumeHistory;
        private int currentIndex = 0;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Analyzes volume patterns to identify institutional activity.
                
Key Features:
- Detects volume spikes above average
- Identifies breakout confirmation
- Filters false signals
- Works with Smart Money Concept

Green = High Volume (Institutional)
Red = Low Volume (Retail)";
                
                Name = "VolumeAnalyzer";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 50;

                LookbackPeriod = 20;
                VolumeThreshold = 1.5;

                // Add plots
                AddPlot(Brushes.LimeGreen, "AverageVolume");
                AddPlot(Brushes.Red, "CurrentVolume");
                AddPlot(Brushes.DodgerBlue, "VolumeRatio");
            }
            else if (State == State.DataLoaded)
            {
                volumeHistory = new double[LookbackPeriod];
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < LookbackPeriod)
                return;

            // Store volume in history array
            volumeHistory[currentIndex % LookbackPeriod] = Volume[0];
            currentIndex++;

            // Calculate average volume
            double avgVolume = volumeHistory.Average();
            double currentVolume = Volume[0];
            double volumeRatio = avgVolume > 0 ? currentVolume / avgVolume : 0;

            // Plot values
            Values[0][0] = avgVolume;
            Values[1][0] = currentVolume;
            Values[2][0] = volumeRatio;

            // Color coding for visual confirmation
            if (volumeRatio >= VolumeThreshold)
            {
                PlotBrushes[0][0] = Brushes.LimeGreen;  // High volume = institutional
                PlotBrushes[1][0] = Brushes.LimeGreen;
            }
            else
            {
                PlotBrushes[0][0] = Brushes.Gray;       // Low volume = retail
                PlotBrushes[1][0] = Brushes.Red;
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(10, 50)]
        [Display(Name = "Lookback Period", Description = "Number of bars to calculate average", GroupName = "Parameters", Order = 1)]
        public int LookbackPeriod
        {
            get { return lookbackPeriod; }
            set { lookbackPeriod = value; }
        }

        [NinjaScriptProperty]
        [Range(0.5, 5.0)]
        [Display(Name = "Volume Threshold", Description = "Multiplier for volume (1.5 = 50% above average)", GroupName = "Parameters", Order = 2)]
        public double VolumeThreshold
        {
            get { return volumeThreshold; }
            set { volumeThreshold = value; }
        }

        #endregion
    }
}
