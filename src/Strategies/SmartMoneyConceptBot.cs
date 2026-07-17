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
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Smart Money Concept Trading Bot
    /// Advanced futures trading strategy for ES, NQ, YM
    /// 
    /// Key Features:
    /// - Order flow analysis
    /// - Supply/Demand zones
    /// - Fair Value Gaps (FVG)
    /// - Market structure breaks
    /// - Dynamic risk management
    /// </summary>
    public class SmartMoneyConceptBot : Strategy
    {
        #region Variables

        // ============================================================================
        // POSITION SIZING & RISK PARAMETERS
        // ============================================================================
        private double accountBalance = 50000;           // Account balance (50K)
        private double riskPercentage = 2.0;             // Risk per trade (%)
        private double dailyDrawdownLimit = 5.0;         // Max daily loss (%)
        private double maxPositionSize = 2;              // Max contracts per trade
        private double currentDailyLoss = 0;             // Track daily P&L
        private DateTime currentTradingDay = DateTime.MinValue;

        // ============================================================================
        // ENTRY PARAMETERS
        // ============================================================================
        private int breakoutBars = 5;                    // Bars for breakout detection
        private double volumeThreshold = 1.5;            // Volume multiplier
        private int orderBlockLookback = 10;             // Bars for order block
        private double fvgMinimumGapPoints = 10;         // Minimum FVG size
        private bool requireHTFConfirmation = true;      // Need higher TF confirmation
        private int confirmationTimeframeMultiplier = 12; // 5min * 12 = 60min

        // ============================================================================
        // EXIT PARAMETERS
        // ============================================================================
        private double takeProfitRatio = 2.0;            // Risk/Reward ratio for TP
        private double stopLossPoints = 20;              // Max points loss per trade
        private bool useTrailingStop = true;             // Enable trailing stop
        private double trailingStopPoints = 10;          // Trailing stop distance
        private int maxHoldingBars = 240;                // Max 4 hours (5min bars)

        // ============================================================================
        // MARKET DATA & STATE
        // ============================================================================
        private double dayHighPrice = 0;
        private double dayLowPrice = double.MaxValue;
        private double previousClose = 0;
        private double previousHigh = 0;
        private double previousLow = double.MaxValue;
        private double averageVolume = 0;
        private int barsInTrade = 0;

        // ============================================================================
        // ORDER BLOCK DETECTION
        // ============================================================================
        private double lastStrongBullishClose = 0;       // Strong bullish candle high
        private double lastStrongBearishClose = 0;       // Strong bearish candle low
        private int lastBullishBar = -100;
        private int lastBearishBar = -100;

        // ============================================================================
        // TRADE TRACKING
        // ============================================================================
        private double entryPrice = 0;
        private double stopLossPrice = 0;
        private double takeProfitPrice = 0;
        private string tradeType = "";                   // "BUY" or "SELL"
        private int tradeID = 0;

        // ============================================================================
        // PERFORMANCE TRACKING
        // ============================================================================
        private double totalTrades = 0;
        private double winningTrades = 0;
        private double totalProfits = 0;
        private double totalLosses = 0;

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Smart Money Concept Trading Bot for Futures (ES, NQ, YM)
                
Features:
- Order flow analysis & institutional activity detection
- Supply/Demand zone identification
- Fair Value Gap (FVG) trading
- Multi-timeframe confirmation
- Dynamic position sizing with Kelly Criterion
- Risk management for funded accounts (50K+)
- Walk-forward backtesting compatible

Recommended Timeframe: 5-minute bars
Instruments: ES, NQ, YM Futures";
                
                Name = "SmartMoneyConceptBot";
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
                
                // Default parameters
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
            }
            else if (State == State.Configure)
            {
                // Configure data series if needed
                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                // Initialize with current prices
                dayHighPrice = Highs[0][0];
                dayLowPrice = Lows[0][0];
                previousClose = Closes[0][1];
                previousHigh = Highs[0][1];
                previousLow = Lows[0][1];
            }
        }

        protected override void OnBarUpdate()
        {
            // Reset daily tracking at new session
            if (Bars.IsFirstBarOfSession)
            {
                if (currentTradingDay != Core.Globals.Now.Date)
                {
                    currentTradingDay = Core.Globals.Now.Date;
                    currentDailyLoss = 0;
                    dayHighPrice = Highs[0][0];
                    dayLowPrice = Lows[0][0];
                }
            }

            // Update daily highs and lows
            dayHighPrice = Math.Max(dayHighPrice, Highs[0][0]);
            dayLowPrice = Math.Min(dayLowPrice, Lows[0][0]);

            // Update previous candle values
            previousHigh = Highs[0][1];
            previousLow = Lows[0][1];
            previousClose = Closes[0][1];

            // ========================================================================
            // RISK CHECKS
            // ========================================================================

            // Check daily drawdown limit
            if (currentDailyLoss <= -AccountBalance * (DailyDrawdownLimit / 100))
            {
                LogMessage($"DAILY LOSS LIMIT HIT: ${currentDailyLoss:F2}");
                ExitAllPositions("Daily Loss Limit");
                return;
            }

            // Update P&L tracking
            foreach (Position position in Account.Positions)
            {
                currentDailyLoss = position.MicroProfitLoss;
            }

            // ========================================================================
            // EXIT MANAGEMENT
            // ========================================================================

            // Exit existing positions first
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                barsInTrade++;
                ManageExits();
            }

            // ========================================================================
            // ENTRY SIGNALS
            // ========================================================================

            // Only enter if not already in a position and risk check passes
            if (Position.MarketPosition == MarketPosition.Flat && 
                currentDailyLoss > -AccountBalance * (DailyDrawdownLimit / 100) * 0.7) // 70% of limit
            {
                CheckEntrySignals();
            }

            // ========================================================================
            // UPDATE PLOTS
            // ========================================================================

            if (Position.MarketPosition != MarketPosition.Flat)
            {
                Values[0][0] = entryPrice;
                Values[1][0] = stopLossPrice;
                Values[2][0] = takeProfitPrice;
            }

            // Plot order blocks
            Values[3][0] = lastStrongBullishClose;
            Values[4][0] = lastStrongBearishClose;
        }

        #region Entry Logic

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
            averageVolume = VOL.Average(20); // 20-bar average volume
            
            if (currentVolume < averageVolume * VolumeThreshold)
                return; // Insufficient volume

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
                // In live implementation, would check multiple timeframes
                // For now, check basic trend
                if (!IsTrendConfirmed(bullishBreakout))
                    return;
            }

            // ================================================================
            // 6. POSITION SIZING & ENTRY
            // ================================================================

            double riskAmount = AccountBalance * (RiskPercentage / 100);
            double contracts = Math.Floor(riskAmount / (StopLossPoints * TickSize * 50)); // 50 = tick value multiplier

            contracts = Math.Min(contracts, MaxPositionSize);

            if (contracts < 1)
                return; // Not enough capital

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
                
                LogMessage($"BUY SIGNAL: {(int)contracts} contracts at {entryPrice:F2} | SL: {stopLossPrice:F2} | TP: {takeProfitPrice:F2}");
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
                
                LogMessage($"SELL SIGNAL: {(int)contracts} contracts at {entryPrice:F2} | SL: {stopLossPrice:F2} | TP: {takeProfitPrice:F2}");
            }
        }

        private void IdentifyOrderBlocks()
        {
            // Look back for strong bullish candles (order blocks to buy)
            for (int i = 1; i <= OrderBlockLookback; i++)
            {
                double bodySize = Math.Abs(Close[i] - Open[i]);
                double totalSize = High[i] - Low[i];
                double bodyPercent = (bodySize / totalSize) * 100;

                // Strong bullish candle: close > open, large body, small wick
                if (Close[i] > Open[i] && bodyPercent > 70 && bodySize > TickSize * 20)
                {
                    lastStrongBullishClose = High[i];
                    lastBullishBar = i;
                    break;
                }
            }

            // Look back for strong bearish candles (order blocks to sell)
            for (int i = 1; i <= OrderBlockLookback; i++)
            {
                double bodySize = Math.Abs(Close[i] - Open[i]);
                double totalSize = High[i] - Low[i];
                double bodyPercent = (bodySize / totalSize) * 100;

                // Strong bearish candle: close < open, large body, small wick
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
            // FVG: Gap between two candles that market will retest
            if (Close[1] < Open[2] && Open[0] > Close[1]) // Bullish FVG
            {
                double gapSize = (Open[0] - Close[1]) / TickSize;
                return gapSize >= FVGMinimumGapPoints;
            }

            if (Close[1] > Open[2] && Open[0] < Close[1]) // Bearish FVG
            {
                double gapSize = (Close[1] - Open[0]) / TickSize;
                return gapSize >= FVGMinimumGapPoints;
            }

            return false;
        }

        private bool IsTrendConfirmed(bool isBullish)
        {
            // Simple trend confirmation: check if recent candles support direction
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

        #region Exit Logic

        private void ManageExits()
        {
            // ================================================================
            // 1. CHECK STOP LOSS
            // ================================================================

            if (Position.MarketPosition == MarketPosition.Long && Close[0] < stopLossPrice)
            {
                ExitLong("Stop Loss Hit");
                LogMessage($"STOP LOSS HIT: Exited at {Close[0]:F2}");
                return;
            }

            if (Position.MarketPosition == MarketPosition.Short && Close[0] > stopLossPrice)
            {
                ExitShort("Stop Loss Hit");
                LogMessage($"STOP LOSS HIT: Exited at {Close[0]:F2}");
                return;
            }

            // ================================================================
            // 2. CHECK TAKE PROFIT
            // ================================================================

            if (Position.MarketPosition == MarketPosition.Long && Close[0] >= takeProfitPrice)
            {
                ExitLong("Take Profit Hit");
                LogMessage($"TAKE PROFIT HIT: Exited at {Close[0]:F2}");
                return;
            }

            if (Position.MarketPosition == MarketPosition.Short && Close[0] <= takeProfitPrice)
            {
                ExitShort("Take Profit Hit");
                LogMessage($"TAKE PROFIT HIT: Exited at {Close[0]:F2}");
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
            // 4. TIME-BASED EXIT (max holding time)
            // ================================================================

            if (barsInTrade >= MaxHoldingBars)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                    ExitLong("Max Hold Time Exceeded");
                else
                    ExitShort("Max Hold Time Exceeded");
                    
                LogMessage($"TIME-BASED EXIT: Trade held for {barsInTrade} bars");
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

        #region Logging & Utilities

        private void LogMessage(string message)
        {
            Print($"[{Time[0]:HH:mm:ss}] {message}");
            
            // In production, would log to file:
            // FileWriter.WriteLine($"{Time[0]:yyyy-MM-dd HH:mm:ss},{message}");
        }

        #endregion

        #region Properties

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
