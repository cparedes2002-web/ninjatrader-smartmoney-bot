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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Order Block Indicator
    /// Identifies institutional order blocks (support/resistance zones)
    /// Used by Smart Money for entry and exit decisions
    /// </summary>
    public class OrderBlockIndicator : Indicator
    {
        private int lookbackPeriod = 10;
        private double minBodyPercent = 70;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Identifies Order Blocks - Strong candles that institutional traders use for entry/exit";
                Name = "OrderBlockIndicator";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 50;

                LookbackPeriod = 10;
                MinBodyPercent = 70;

                AddPlot(Brushes.LimeGreen, "BullishBlock");
                AddPlot(Brushes.Red, "BearishBlock");
            }
        }

        protected override void OnBarUpdate()
        {
            // Scan for strong bullish candles (Order Blocks to Buy)
            for (int i = 1; i <= LookbackPeriod; i++)
            {
                if (i >= CurrentBar) break;

                double bodySize = Math.Abs(Close[i] - Open[i]);
                double totalSize = High[i] - Low[i];
                double bodyPercent = totalSize > 0 ? (bodySize / totalSize) * 100 : 0;

                // Strong bullish candle
                if (Close[i] > Open[i] && bodyPercent >= MinBodyPercent)
                {
                    Values[0][0] = High[i];
                    Draw.Line(this, "BullishBlock_" + i, false, i, High[i], 0, High[i], 
                              Brushes.LimeGreen);
                }

                // Strong bearish candle
                if (Close[i] < Open[i] && bodyPercent >= MinBodyPercent)
                {
                    Values[1][0] = Low[i];
                    Draw.Line(this, "BearishBlock_" + i, false, i, Low[i], 0, Low[i], 
                              Brushes.Red);
                }
            }
        }

        [NinjaScriptProperty]
        [Range(5, 50)]
        [Display(Name = "Lookback Period", GroupName = "Parameters", Order = 1)]
        public int LookbackPeriod
        {
            get { return lookbackPeriod; }
            set { lookbackPeriod = value; }
        }

        [NinjaScriptProperty]
        [Range(50, 100)]
        [Display(Name = "Min Body Percent", GroupName = "Parameters", Order = 2)]
        public double MinBodyPercent
        {
            get { return minBodyPercent; }
            set { minBodyPercent = value; }
        }
    }
}
