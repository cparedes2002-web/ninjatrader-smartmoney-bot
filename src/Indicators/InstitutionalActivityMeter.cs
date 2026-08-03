using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Institutional Activity Meter
    /// Detects Smart Money (institutional) buying and selling pressure
    /// Combines order flow, volume, and price action analysis
    /// 
    /// Signals:
    /// - Green histogram = Institutional buying
    /// - Red histogram = Institutional selling
    /// - Height = Strength of activity
    /// </summary>
    public class InstitutionalActivityMeter : Indicator
    {
        private int volumePeriod = 20;
        private int pricePeriod = 14;
        private double volumeMultiplier = 1.5;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Institutional Activity Meter

Detects Smart Money Concept activity by analyzing:
- Volume above average (institutional activity)
- Price momentum (directional pressure)
- Rate of change (acceleration)

Green = Buying pressure (institutions buying)
Red = Selling pressure (institutions selling)
Height = Strength of activity";
                
                Name = "InstitutionalActivityMeter";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 50;

                VolumePeriod = 20;
                PricePeriod = 14;
                VolumeMultiplier = 1.5;

                AddPlot(Brushes.Transparent, "InstitutionalActivity");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(VolumePeriod, PricePeriod))
                return;

            // Calculate average volume
            double avgVolume = VOL.Average(VolumePeriod);
            double currentVolume = Volume[0];
            double volumeRatio = avgVolume > 0 ? currentVolume / avgVolume : 0;

            // Calculate price momentum (ROC)
            double currentPrice = Close[0];
            double previousPrice = Close[PricePeriod];
            double priceChange = previousPrice != 0 ? ((currentPrice - previousPrice) / previousPrice) * 100 : 0;

            // Combine volume and price signals
            double institutionalScore = 0;

            if (volumeRatio >= VolumeMultiplier)
            {
                if (Close[0] > Open[0]) // Bullish with high volume = buying
                    institutionalScore = volumeRatio * Math.Abs(priceChange);
                else if (Close[0] < Open[0]) // Bearish with high volume = selling
                    institutionalScore = -volumeRatio * Math.Abs(priceChange);
            }
            else
            {
                institutionalScore = priceChange * 0.5; // Weak signal without volume
            }

            Values[0][0] = institutionalScore;

            // Color the histogram
            if (institutionalScore > 0)
                PlotBrushes[0][0] = Brushes.LimeGreen;
            else if (institutionalScore < 0)
                PlotBrushes[0][0] = Brushes.Red;
            else
                PlotBrushes[0][0] = Brushes.Gray;
        }

        [NinjaScriptProperty]
        [Range(10, 50)]
        [Display(Name = "Volume Period", GroupName = "Parameters", Order = 1)]
        public int VolumePeriod
        {
            get { return volumePeriod; }
            set { volumePeriod = value; }
        }

        [NinjaScriptProperty]
        [Range(5, 30)]
        [Display(Name = "Price Period", GroupName = "Parameters", Order = 2)]
        public int PricePeriod
        {
            get { return pricePeriod; }
            set { pricePeriod = value; }
        }

        [NinjaScriptProperty]
        [Range(0.5, 5.0)]
        [Display(Name = "Volume Multiplier", GroupName = "Parameters", Order = 3)]
        public double VolumeMultiplier
        {
            get { return volumeMultiplier; }
            set { volumeMultiplier = value; }
        }
    }
}
