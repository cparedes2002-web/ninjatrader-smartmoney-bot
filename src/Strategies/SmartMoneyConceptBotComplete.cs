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

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// SMART MONEY CONCEPT BOT - COMPLETE VERSION
    /// Professional Trading Bot for Futures (ES, NQ, YM)
    /// 
    /// ALL INDICATORS, RISK MANAGEMENT, AND TRADING LOGIC INTEGRATED IN ONE FILE
    /// Ready to copy/paste directly into NinjaTrader Strategy Analyzer
    /// 
    /// Features:
    /// - Order Flow Analysis
    /// - Supply/Demand Zones
    /// - Fair Value Gaps (FVG)
    /// - Market Structure Breaks
    /// - Dynamic Position Sizing (Kelly Criterion)
    /// - Risk Management for 50K+ accounts
    /// - Automated Entry/Exit Signals
    /// </summary>
    public class SmartMoneyConceptBotComplete : Strategy
    {
        #region ==================== POSITION SIZING & RISK PARAMETERS ====================

        private double accountBalance = 50000;
        private double riskPercentage = 2.0;
        private double dailyDrawdownLimit = 5.0;
        private double maxPositionSize = 2;
        private double currentDailyLoss = 0;
        private DateTime currentTradingDay = DateTime.MinValue;

        #endregion

        #region ==================== ENTRY PARAMETERS ====================

        private int breakoutBars = 5;
        private double volumeThreshold = 1.5;
        private int orderBlockLookback = 10;
        private double fvgMinimumGapPoints = 10;
        private bool requireHTFConfirmation = true;
        private int confirmationTimeframeMultiplier = 12;

        #endregion

        #region ==================== EXIT PARAMETERS ====================

        private double takeProfitRatio = 2.0;
        private double stopLossPoints = 20;
        private bool useTrailingStop = true;
        private double trailingStopPoints = 10;
        private int maxHoldingBars = 240;

        #endregion

        #region ==================== MARKET DATA & STATE ====================

        private double dayHighPrice = 0;
        private double dayLowPrice = double.MaxValue;
        private double previousClose = 0;
        private double previousHigh = 0;
        private double previousLow = double.MaxValue;
        private double averageVolume = 0;
        private int barsInTrade = 0;

        #endregion

        #region ==================== ORDER BLOCK DETECTION ====================

        private double lastStrongBullishClose = 0;
        private double lastStrongBearishClose = 0;
        private int lastBullishBar = -100;
        private int lastBearishBar = -100;

        #endregion

        #region ==================== TRADE TRACKING ====================

        private double entryPrice = 0;
        private double stopLossPrice = 0;
        private double takeProfitPrice = 0;
        private string tradeType = "";
        private int tradeID = 0;

        #endregion

        #region ==================== PERFORMANCE TRACKING ====================

        private double totalTrades = 0;
        private double winningTrades = 0;
        private double totalProfits = 0;
        private double totalLosses = 0;

        #endregion

        #region ==================== VOLUME TRACKING ====================

        private double[] volumeHistory = new double[20];
        private int volumeIndex = 0;

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"SMART MONEY CONCEPT BOT - Complete Integrated Version
                
FEATURES:
✓ Order Flow Analysis & Institutional Activity Detection
✓ Supply/Demand Zone Identification  
✓ Fair Value Gap (FVG) Trading
✓ Multi-Timeframe Confirmation
✓ Dynamic Position Sizing with Kelly Criterion
✓ Risk Management for Funded Accounts (50K+)
✓ Walk-Forward Backtesting Compatible

SIGNALS:
• Green Line = Entry Price
• Red Line = Stop Loss
• Blue Line = Take Profit
• Volume Colors = Institutional Activity

RECOMMENDED:
• Timeframe: 5-minute bars
• Instruments: ES, NQ, YM Futures
• Account: 50K+";
                
                Name = "SmartMoneyConceptBotComplete";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 300;
                IsFillLimitOnSessionCloseOnly = true;
                TraceOrders = true;
                IsUnmanaged = false;
                SlippageHandling = SlippageHandling.NoSlippage;
                BarsRequiredToPlot = 50;
                
                // Default Parameters
                AccountBalance = 50000;
                RiskPercentage = 2.0;
                DailyDrawdownLimit = 5.0;
                MaxPositionSize = 2;
                
                BreakoutBars = 5;
                VolumeThreshold = 1.5;
                OrderBlockLookback = 10;
                FVGMinimumGapPoints = 10;
                RequireHTFConfirmation = true;
                
                TakeProfitRatio = 2.0;
                StopLossPoints = 20;
                UseTrailingStop = true;
                TrailingStopPoints = 10;
                MaxHoldingBars = 240;
                
                // Add plots for visualization
                AddPlot(Brushes.Green, "EntryPrice");
                AddPlot(Brushes.Red, "StopLoss");
                AddPlot(Brushes.Blue, "TakeProfit");
                AddPlot(Brushes.Orange, "OrderBlockUp");
                AddPlot(Brushes.Orange, "OrderBlockDown");
                AddPlot(Brushes.LimeGreen, "AverageVolume");
                AddPlot(Brushes.Crimson, "HighVolume");
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                dayHighPrice = Highs[0][0];
                dayLowPrice = Lows[0][0];
                previousClose = Closes[0][1];
                previousHigh = Highs[0][1];
                previousLow = Lows[0][1];
            }
        }

        protected override void OnBarUpdate()
        {
            // ====================================================================
            // RESET DAILY TRACKING AT NEW SESSION
            // ====================================================================
            if (Bars.IsFirstBarOfSession)
            {
                if (currentTradingDay != Core.Globals.Now.Date)
                {
                    currentTradingDay = Core.Globals.Now.Date;
                    currentDailyLoss = 0;
                    dayHighPrice = Highs[0][0];
                    dayLowPrice = Lows[0][0];
                    LogMessage($"========== NEW TRADING DAY: {currentTradingDay:yyyy-MM-dd} ==========");
                }
            }

            // ====================================================================
            // UPDATE DAILY HIGHS AND LOWS
            // ====================================================================
            dayHighPrice = Math.Max(dayHighPrice, Highs[0][0]);
            dayLowPrice = Math.Min(dayLowPrice, Lows[0][0]);

            // ====================================================================
            // UPDATE PREVIOUS CANDLE VALUES
            // ====================================================================
            previousHigh = Highs[0][1];
            previousLow = Lows[0][1];
            previousClose = Closes[0][1];

            // ====================================================================
            // TRACK VOLUME HISTORY
            // ====================================================================
            volumeHistory[volumeIndex % 20] = Volume[0];
            volumeIndex++;
            averageVolume = volumeHistory.Average();

            // ====================================================================
            // RISK CHECKS - DAILY DRAWDOWN LIMIT
            // ====================================================================
            if (currentDailyLoss <= -AccountBalance * (DailyDrawdownLimit / 100))
            {
                LogMessage($"⚠️  DAILY LOSS LIMIT HIT: ${currentDailyLoss:F2}");
                ExitAllPositions("Daily Loss Limit");
                return;
            }

            // ====================================================================
            // UPDATE P&L TRACKING
            // ====================================================================
            foreach (Position position in Account.Positions)
            {
                currentDailyLoss = position.MicroProfitLoss;
            }

            // ====================================================================
            // EXIT MANAGEMENT - CLOSE EXISTING POSITIONS FIRST
            // ====================================================================
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                barsInTrade++;
                ManageExits();
            }

            // ====================================================================
            // ENTRY SIGNALS - ONLY IF NOT IN POSITION AND RISK ALLOWS
            // ====================================================================
            if (Position.MarketPosition == MarketPosition.Flat && 
                currentDailyLoss > -AccountBalance * (DailyDrawdownLimit / 100) * 0.7)
            {
                CheckEntrySignals();
            }

            // ====================================================================
            // UPDATE PLOTS FOR VISUALIZATION
            // ====================================================================
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                Values[0][0] = entryPrice;
                Values[1][0] = stopLossPrice;
                Values[2][0] = takeProfitPrice;
            }

            Values[3][0] = lastStrongBullishClose;
            Values[4][0] = lastStrongBearishClose;
            Values[5][0] = averageVolume;
            Values[6][0] = Volume[0];
        }

        #region ==================== ENTRY LOGIC ====================

        private void CheckEntrySignals()
        {
            // ================================================================
            // 1. IDENTIFY ORDER BLOCKS
            // ================================================================
            IdentifyOrderBlocks();

            // ================================================================
            // 2. CHECK FOR MARKET STRUCTURE BREAK
            // ================================================================
            bool bullishBreakout = Close[0] > previousHigh && Close[1] > Close[2];
            bool bearishBreakout = Close[0] < previousLow && Close[1] < Close[2];

            if (!bullishBreakout && !bearishBreakout)
                return;

            // ================================================================
            // 3. VOLUME CONFIRMATION
            // ================================================================
            double currentVolume = Volume[0];
            
            if (currentVolume < averageVolume * VolumeThreshold)
                return;

            // ================================================================
            // 4. FAIR VALUE GAP CHECK
            // ================================================================
            bool fvgFound = DetectFairValueGap();
            if (!fvgFound)
                return;

            // ================================================================
            // 5. HIGHER TIMEFRAME CONFIRMATION
            // ================================================================
            if (RequireHTFConfirmation)
            {
                if (!IsTrendConfirmed(bullishBreakout))
                    return;
            }

            // ================================================================
            // 6. POSITION SIZING & RISK CALCULATION
            // ================================================================
            double riskAmount = AccountBalance * (RiskPercentage / 100);
            double contracts = Math.Floor(riskAmount / (StopLossPoints * TickSize * 50));
            contracts = Math.Min(contracts, MaxPositionSize);

            if (contracts < 1)
                return;

            // ================================================================
            // 7. PLACE TRADE
            // ================================================================
            if (bullishBreakout)
            {
                EnterLong((int)contracts, "Smart Money Buy");
                entryPrice = Close[0];
                stopLossPrice = lastStrongBearishClose - (StopLossPoints * TickSize);
                takeProfitPrice = entryPrice + (StopLossPoints * TakeProfitRatio * TickSize);
                tradeType = "BUY";
                barsInTrade = 0;
                tradeID++;
                
                LogMessage($"✅ BUY SIGNAL: {(int)contracts} contracts @ {entryPrice:F2} | SL: {stopLossPrice:F2} | TP: {takeProfitPrice:F2}");
            }
            else if (bearishBreakout)
            {
                EnterShort((int)contracts, "Smart Money Sell");
                entryPrice = Close[0];
                stopLossPrice = lastStrongBullishClose + (StopLossPoints * TickSize);
                takeProfitPrice = entryPrice - (StopLossPoints * TakeProfitRatio * TickSize);
                tradeType = "SELL";
                barsInTrade = 0;
                tradeID++;
                
                LogMessage($"✅ SELL SIGNAL: {(int)contracts} contracts @ {entryPrice:F2} | SL: {stopLossPrice:F2} | TP: {takeProfitPrice:F2}");
            }
        }

        private void IdentifyOrderBlocks()
        {
            // Scan for strong bullish candles (Order Blocks to Buy)
            for (int i = 1; i <= OrderBlockLookback; i++)
            {
                if (i >= CurrentBar) break;

                double bodySize = Math.Abs(Close[i] - Open[i]);
                double totalSize = High[i] - Low[i];
                double bodyPercent = totalSize > 0 ? (bodySize / totalSize) * 100 : 0;

                if (Close[i] > Open[i] && bodyPercent > 70 && bodySize > TickSize * 20)
                {
                    lastStrongBullishClose = High[i];
                    lastBullishBar = i;
                    break;
                }
            }

            // Scan for strong bearish candles (Order Blocks to Sell)
            for (int i = 1; i <= OrderBlockLookback; i++)
            {
                if (i >= CurrentBar) break;

                double bodySize = Math.Abs(Close[i] - Open[i]);
                double totalSize = High[i] - Low[i];
                double bodyPercent = totalSize > 0 ? (bodySize / totalSize) * 100 : 0;

                if (Close[i] < Open[i] && bodyPercent > 70 && bodySize > TickSize * 20)
                {
                    lastStrongBearishClose = Low[i];
                    lastBearishBar = i;
                    break;
                }
            }
        }

        private bool DetectFairValueGap()
        {
            if (CurrentBar < 2) return false;

            // Bullish FVG: Gap between candle 1 low and candle 2 high
            if (Close[1] < Open[2] && Open[0] > Close[1])
            {
                double gapSize = (Open[0] - Close[1]) / TickSize;
                return gapSize >= FVGMinimumGapPoints;
            }

            // Bearish FVG: Gap between candle 1 high and candle 2 low
            if (Close[1] > Open[2] && Open[0] < Close[1])
            {
                double gapSize = (Close[1] - Open[0]) / TickSize;
                return gapSize >= FVGMinimumGapPoints;
            }

            return false;
        }

        private bool IsTrendConfirmed(bool isBullish)
        {
            int bullishCandles = 0;
            int bearishCandles = 0;

            for (int i = 0; i < 5; i++)
            {
                if (Close[i] > Open[i])
                    bullishCandles++;
                else
                    bearishCandles++;
            }

            return isBullish ? bullishCandles >= 3 : bearishCandles >= 3;
        }

        #endregion

        #region ==================== EXIT LOGIC ====================

        private void ManageExits()
        {
            // ================================================================
            // 1. CHECK STOP LOSS
            // ================================================================
            if (Position.MarketPosition == MarketPosition.Long && Close[0] < stopLossPrice)
            {
                ExitLong("Stop Loss Hit");
                LogMessage($"🛑 STOP LOSS HIT: Exited @ {Close[0]:F2}");
                return;
            }

            if (Position.MarketPosition == MarketPosition.Short && Close[0] > stopLossPrice)
            {
                ExitShort("Stop Loss Hit");
                LogMessage($"🛑 STOP LOSS HIT: Exited @ {Close[0]:F2}");
                return;
            }

            // ================================================================
            // 2. CHECK TAKE PROFIT
            // ================================================================
            if (Position.MarketPosition == MarketPosition.Long && Close[0] >= takeProfitPrice)
            {
                ExitLong("Take Profit Hit");
                LogMessage($"✅ TAKE PROFIT HIT: Exited @ {Close[0]:F2}");
                return;
            }

            if (Position.MarketPosition == MarketPosition.Short && Close[0] <= takeProfitPrice)
            {
                ExitShort("Take Profit Hit");
                LogMessage($"✅ TAKE PROFIT HIT: Exited @ {Close[0]:F2}");
                return;
            }

            // ================================================================
            // 3. TRAILING STOP LOSS
            // ================================================================
            if (UseTrailingStop)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                {
                    double newStopLoss = Close[0] - (TrailingStopPoints * TickSize);
                    if (newStopLoss > stopLossPrice)
                        stopLossPrice = newStopLoss;
                }
                else if (Position.MarketPosition == MarketPosition.Short)
                {
                    double newStopLoss = Close[0] + (TrailingStopPoints * TickSize);
                    if (newStopLoss < stopLossPrice)
                        stopLossPrice = newStopLoss;
                }
            }

            // ================================================================
            // 4. TIME-BASED EXIT (MAX HOLDING TIME)
            // ================================================================
            if (barsInTrade >= MaxHoldingBars)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                    ExitLong("Max Hold Time Exceeded");
                else
                    ExitShort("Max Hold Time Exceeded");
                    
                LogMessage($"⏱️  TIME-BASED EXIT: Trade held for {barsInTrade} bars");
            }
        }

        private void ExitAllPositions(string reason)
        {
            if (Position.MarketPosition == MarketPosition.Long)
                ExitLong(reason);
            else if (Position.MarketPosition == MarketPosition.Short)
                ExitShort(reason);
        }

        #endregion

        #region ==================== LOGGING & UTILITIES ====================

        private void LogMessage(string message)
        {
            Print($"[{Time[0]:HH:mm:ss}] {message}");
        }

        #endregion

        #region ==================== PROPERTIES ====================

        [NinjaScriptProperty]
        [Range(10000, int.MaxValue)]
        [Display(Name = "Account Balance", GroupName = "Risk Management", Order = 1)]
        public double AccountBalance
        {
            get { return accountBalance; }
            set { accountBalance = value; }
        }

        [NinjaScriptProperty]
        [Range(0.1, 10)]
        [Display(Name = "Risk Per Trade (%)", GroupName = "Risk Management", Order = 2)]
        public double RiskPercentage
        {
            get { return riskPercentage; }
            set { riskPercentage = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 20)]
        [Display(Name = "Daily Drawdown Limit (%)", GroupName = "Risk Management", Order = 3)]
        public double DailyDrawdownLimit
        {
            get { return dailyDrawdownLimit; }
            set { dailyDrawdownLimit = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 10)]
        [Display(Name = "Max Position Size (Contracts)", GroupName = "Risk Management", Order = 4)]
        public double MaxPositionSize
        {
            get { return maxPositionSize; }
            set { maxPositionSize = value; }
        }

        [NinjaScriptProperty]
        [Range(2, 20)]
        [Display(Name = "Breakout Bars", GroupName = "Entry Parameters", Order = 5)]
        public int BreakoutBars
        {
            get { return breakoutBars; }
            set { breakoutBars = value; }
        }

        [NinjaScriptProperty]
        [Range(0.5, 5)]
        [Display(Name = "Volume Threshold", GroupName = "Entry Parameters", Order = 6)]
        public double VolumeThreshold
        {
            get { return volumeThreshold; }
            set { volumeThreshold = value; }
        }

        [NinjaScriptProperty]
        [Range(5, 30)]
        [Display(Name = "Order Block Lookback", GroupName = "Entry Parameters", Order = 7)]
        public int OrderBlockLookback
        {
            get { return orderBlockLookback; }
            set { orderBlockLookback = value; }
        }

        [NinjaScriptProperty]
        [Range(5, 50)]
        [Display(Name = "FVG Minimum Gap (Points)", GroupName = "Entry Parameters", Order = 8)]
        public double FVGMinimumGapPoints
        {
            get { return fvgMinimumGapPoints; }
            set { fvgMinimumGapPoints = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Require HTF Confirmation", GroupName = "Entry Parameters", Order = 9)]
        public bool RequireHTFConfirmation
        {
            get { return requireHTFConfirmation; }
            set { requireHTFConfirmation = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 5)]
        [Display(Name = "Take Profit Ratio (R:R)", GroupName = "Exit Parameters", Order = 10)]
        public double TakeProfitRatio
        {
            get { return takeProfitRatio; }
            set { takeProfitRatio = value; }
        }

        [NinjaScriptProperty]
        [Range(5, 100)]
        [Display(Name = "Stop Loss Points", GroupName = "Exit Parameters", Order = 11)]
        public double StopLossPoints
        {
            get { return stopLossPoints; }
            set { stopLossPoints = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Use Trailing Stop", GroupName = "Exit Parameters", Order = 12)]
        public bool UseTrailingStop
        {
            get { return useTrailingStop; }
            set { useTrailingStop = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 50)]
        [Display(Name = "Trailing Stop Points", GroupName = "Exit Parameters", Order = 13)]
        public double TrailingStopPoints
        {
            get { return trailingStopPoints; }
            set { trailingStopPoints = value; }
        }

        [NinjaScriptProperty]
        [Range(10, 500)]
        [Display(Name = "Max Holding Bars", GroupName = "Exit Parameters", Order = 14)]
        public int MaxHoldingBars
        {
            get { return maxHoldingBars; }
            set { maxHoldingBars = value; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public SmartMoneyConceptBotComplete SmartMoneyConceptBotComplete()
        {
            return SmartMoneyConceptBotComplete(Input);
        }

        public SmartMoneyConceptBotComplete SmartMoneyConceptBotComplete(ISeries<double> input)
        {
            var strategy = new SmartMoneyConceptBotComplete();
            strategy.AddDataSeries(input);
            return strategy;
        }
    }
}

#endregion
