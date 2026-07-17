#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Fair Value Gap (FVG) Indicator
    /// Detects imbalances in price where the market is likely to retrace
    /// Key concept in Smart Money trading
    /// </summary>
    public class FairValueGapIndicator : Indicator
    {
        private double minGapPoints = 10;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Identifies Fair Value Gaps - Price imbalances that smart money exploits";
                Name = "FairValueGapIndicator";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 50;

                MinGapPoints = 10;

                AddPlot(Brushes.DodgerBlue, "BullishFVG");
                AddPlot(Brushes.OrangeRed, "BearishFVG");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 2) return;

            double gapSize = 0;

            // Bullish FVG: Gap between candle 1 low and candle 2 high
            if (Close[1] < Open[2] && Open[0] > Close[1])
            {
                gapSize = (Open[0] - Close[1]) / TickSize;
                if (gapSize >= MinGapPoints)
                {
                    Values[0][0] = (Open[0] + Close[1]) / 2.0;
                    Draw.Rectangle(this, "BullishFVG_" + CurrentBar, false, 2, Close[1], 0, Open[0], 
                                  Brushes.Transparent, Brushes.DodgerBlue, 2);
                }
            }

            // Bearish FVG: Gap between candle 1 high and candle 2 low
            if (Close[1] > Open[2] && Open[0] < Close[1])
            {
                gapSize = (Close[1] - Open[0]) / TickSize;
                if (gapSize >= MinGapPoints)
                {
                    Values[1][0] = (Close[1] + Open[0]) / 2.0;
                    Draw.Rectangle(this, "BearishFVG_" + CurrentBar, false, 2, Open[0], 0, Close[1], 
                                  Brushes.Transparent, Brushes.OrangeRed, 2);
                }
            }
        }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Min Gap Points", GroupName = "Parameters", Order = 1)]
        public double MinGapPoints
        {
            get { return minGapPoints; }
            set { minGapPoints = value; }
        }
    }
}
