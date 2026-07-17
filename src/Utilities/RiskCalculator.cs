using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;

namespace NinjaTrader.NinjaScript.Strategies.SmartMoney
{
    /// <summary>
    /// Risk Management Calculator
    /// Handles position sizing, Kelly Criterion, and risk calculations
    /// </summary>
    public class RiskCalculator
    {
        private double accountBalance;
        private double riskPercentage;
        private double maxPositionSize;
        private double winRate;
        private double avgWin;
        private double avgLoss;

        public RiskCalculator(double balance, double riskPct, double maxPos)
        {
            accountBalance = balance;
            riskPercentage = riskPct;
            maxPositionSize = maxPos;
        }

        /// <summary>
        /// Calculate position size using Fixed Fractional method
        /// Formula: (Account × Risk%) / (SL Points × Tick Value)
        /// </summary>
        public int CalculatePositionSize(double stopLossPoints, double tickValue, double pointValue)
        {
            double riskAmount = accountBalance * (riskPercentage / 100.0);
            double positionValue = stopLossPoints * pointValue;
            double contracts = Math.Floor(riskAmount / positionValue);
            
            return (int)Math.Min(contracts, maxPositionSize);
        }

        /// <summary>
        /// Calculate Kelly Criterion for position sizing
        /// Formula: (BP - Q) / B
        /// B = odds, P = win probability, Q = loss probability
        /// </summary>
        public double CalculateKellyCriterion(double winProbability, double rewardRatio)
        {
            double lossProbability = 1.0 - winProbability;
            double kellyCriterion = (rewardRatio * winProbability - lossProbability) / rewardRatio;
            
            // Apply Kelly Fraction (typically 0.25 for conservative trading)
            double kelliFraction = 0.25;
            return Math.Max(0, kellyCriterion * kelliFraction);
        }

        /// <summary>
        /// Calculate take profit level based on risk/reward ratio
        /// </summary>
        public double CalculateTakeProfit(double entryPrice, double stopLossPrice, double riskRewardRatio, bool isLong)
        {
            double riskAmount = Math.Abs(entryPrice - stopLossPrice);
            double targetProfit = riskAmount * riskRewardRatio;
            
            return isLong ? entryPrice + targetProfit : entryPrice - targetProfit;
        }

        /// <summary>
        /// Calculate maximum daily loss threshold
        /// </summary>
        public double CalculateDailyLossLimit(double maxDailyDrawdownPercent)
        {
            return accountBalance * (maxDailyDrawdownPercent / 100.0);
        }

        /// <summary>
        /// Verify if trade meets minimum profitability requirements
        /// </summary>
        public bool IsTradeViable(double entryPrice, double stopPrice, double takeProfitPrice, 
                                  double minRiskReward = 1.5, double maxRiskPercent = 2.0)
        {
            double risk = Math.Abs(entryPrice - stopPrice);
            double reward = Math.Abs(takeProfitPrice - entryPrice);
            double riskRewardRatio = reward / risk;
            double riskAmount = risk / entryPrice * 100;

            return riskRewardRatio >= minRiskReward && riskAmount <= maxRiskPercent;
        }
    }
}
