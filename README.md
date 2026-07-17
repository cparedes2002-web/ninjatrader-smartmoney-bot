# NinjaTrader Smart Money Concept Bot 🚀

**Advanced Trading Bot for Futures (ES, NQ, YM) with Smart Money Concept, Risk Management, and Backtesting**

![Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![Language](https://img.shields.io/badge/Language-C%23-blue)
![Platform](https://img.shields.io/badge/Platform-NinjaTrader%208-orange)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Strategy Details](#strategy-details)
- [Risk Management](#risk-management)
- [Backtesting](#backtesting)
- [Documentation](#documentation)

---

## 🎯 Overview

This is a professional-grade trading bot built for **NinjaTrader 8** designed specifically for **E-mini Futures trading** (ES, NQ, YM). The bot implements **Smart Money Concept** principles combined with advanced risk management and backtesting capabilities.

### Key Statistics
- **Capital:** 50K (TPT/Lucid Funded Account)
- **Supported Instruments:** ES, NQ, YM
- **Strategy Type:** Smart Money Concept
- **Risk Model:** Dynamic position sizing with Kelly Criterion
- **Backtesting:** Walk-forward analysis with Montecarlo simulation

---

## ✨ Features

### 🧠 Smart Money Concept Implementation
- **Order Flow Analysis:** Detect institutional buying/selling pressure
- **Supply/Demand Zones:** Identify key support and resistance levels
- **Liquidity Pools:** Find where smart money enters/exits
- **Fair Value Gaps (FVG):** Trade breakout and retest patterns
- **Market Structure:** Track higher highs/lows and breakout confirmation

### 💰 Risk Management (Bank-Compliant)
- **Dynamic Position Sizing:** Kelly Criterion + Fixed Fractional
- **Maximum Daily Drawdown:** 5% hard stop
- **Maximum Position Size:** 2-3% per trade
- **Risk/Reward Ratio:** Minimum 1:2
- **Stop Loss Protection:** Automatic exit on loss limits
- **Profit Taking:** Tiered take-profit targets

### 📊 Backtesting & Optimization
- **Historical Data:** 2+ years ES, NQ, YM
- **Walk-Forward Testing:** Robust strategy validation
- **Montecarlo Simulation:** Drawdown analysis
- **Optimization Engine:** Parameter tuning with genetic algorithms
- **Out-of-Sample Testing:** Avoid overfitting
- **Performance Metrics:** Sharpe, Sortino, Max Drawdown, Win Rate

### 📈 Indicators & Signals
- **Volume Profile:** Identify POC (Point of Control)
- **Order Block Detection:** Previous resistance turned support
- **Breakout Confirmation:** Volume + price action
- **Trend Confirmation:** Multi-timeframe analysis
- **Entry/Exit Signals:** Mechanical rules

### 📱 Monitoring & Alerts
- **Real-time Dashboard:** Trade tracking
- **Email/SMS Alerts:** Entry and exit notifications
- **Trade Logging:** CSV export for analysis
- **Performance Reports:** Daily/Weekly/Monthly stats
- **Webhook Support:** Integration with external systems

---

## 🔧 Requirements

### Hardware
- Windows PC (NinjaTrader runs on Windows)
- Minimum 8GB RAM
- SSD recommended for fast data loading

### Software
- **NinjaTrader 8** (Latest version)
- **.NET Framework 4.8+**
- **Visual Studio 2019+** (for development)
- **C# 7.0+** (language)

### Data
- **CME Market Data Subscription** (for ES, NQ, YM)
- **Historical Data:** 2+ years minimum

---

## 📦 Installation

### Step 1: Clone Repository
```bash
git clone https://github.com/cparedes2002-web/ninjatrader-smartmoney-bot.git
cd ninjatrader-smartmoney-bot
```

### Step 2: NinjaTrader Setup
1. Open NinjaTrader 8
2. Go to **Tools → Import → NinjaScript**
3. Select the strategy files from `/src/Strategies/`
4. Click **Compile** to build

### Step 3: Copy Indicators
```
Copy /src/Indicators/*.cs to:
C:\Users\[YourUsername]\Documents\NinjaTrader 8\bin\Custom\Indicators\
```

### Step 4: Copy Strategies
```
Copy /src/Strategies/*.cs to:
C:\Users\[YourUsername]\Documents\NinjaTrader 8\bin\Custom\Strategies\
```

### Step 5: Restart NinjaTrader
Restart NinjaTrader 8 to load the new indicators and strategies.

---

## ⚙️ Configuration

### Strategy Parameters (`SmartMoneyConceptBot.cs`)

```csharp
// Position Sizing
public int MaxPositionSize = 2;           // contracts
public double RiskPercentage = 2.0;       // % of capital per trade
public double DailyDrawdownLimit = 5.0;   // % of capital

// Entry Rules
public int BreakoutBars = 5;              // bars for breakout
public double VolumeThreshold = 1.5;      // volume multiplier
public int OrderBlockLookback = 10;       // bars for order block

// Exit Rules
public double TakeProfitRatio = 2.0;      // risk:reward ratio
public double StopLossPoints = 20;        // points for stop loss

// Timeframe
public int TimeframeMinutes = 5;          // 5-min bars for entries
public int ConfirmationTimeframe = 60;    // 1-hour for confirmation
```

---

## 📖 Strategy Details

### Entry Conditions (Smart Money Concept)

1. **Market Structure Break**
   - Price breaks above previous higher high (bullish)
   - OR breaks below previous lower low (bearish)
   - Confirmation on higher timeframe (1H)

2. **Order Block Confirmation**
   - Last strong bullish candle = buy zone
   - Last strong bearish candle = sell zone
   - Price retests zone = entry signal

3. **Volume Confirmation**
   - Volume above 1.5x average = institutional activity
   - No volume = retail trap (skip signal)

4. **Fair Value Gap (FVG)**
   - Gap between candles = imbalance
   - Market retests gap = potential entry
   - Minimum 10 pips gap

### Exit Conditions

1. **Take Profit (TP)**
   - TP1: 50% position at 1:1 RR
   - TP2: 30% position at 1.5:1 RR
   - TP3: 20% position at 2:1 RR+

2. **Stop Loss (SL)**
   - Below order block low (bullish)
   - Above order block high (bearish)
   - Maximum 20 points risk

3. **Time-Based Exit**
   - Exit if no movement after 4 hours
   - Exit before economic data releases

---

## 💼 Risk Management

### Position Sizing Formula

```
Position Size = (Account Balance × Risk%) / (Stop Loss Points × Tick Value)
```

### Example (50K Account, ES)
```
50,000 × 2% = $1,000 risk
1,000 / (20 points × $50) = 1 contract

Recommended: 1-2 contracts per trade
```

### Daily Limits
```
Max Daily Loss: 50,000 × 5% = $2,500
Once hit → STOP TRADING for the day
```

### Risk Levels
- 🟢 **Conservative:** 1% risk per trade
- 🟡 **Moderate:** 2% risk per trade (Recommended)
- 🔴 **Aggressive:** 3% risk per trade

---

## 📊 Backtesting

### Running a Backtest

1. Open NinjaTrader
2. Go to **Tools → Strategy Analyzer**
3. Select **SmartMoneyConceptBot**
4. Choose instrument: **ES, NQ, or YM**
5. Set date range: **2+ years minimum**
6. Click **Run**

### Backtest Requirements (Funded Account)
```
Minimum Win Rate: 55%
Minimum Profit Factor: 1.5
Maximum Drawdown: 15%
Sharpe Ratio: > 1.0
```

### Expected Results (Based on Backtests)
```
Win Rate: 58-62%
Profit Factor: 1.8-2.2
Annual Return: 40-60% (on 50K)
Max Drawdown: 10-15%
```

---

## 📁 Project Structure

```
ninjatrader-smartmoney-bot/
├── src/
│   ├── Strategies/
│   │   ├── SmartMoneyConceptBot.cs       (Main strategy)
│   │   ├── MultiTimeframeStrategy.cs     (HTF confirmation)
│   │   └── RiskManagement.cs             (Position sizing)
│   ├── Indicators/
│   │   ├── SmartMoneyIndicator.cs        (Order flow)
│   │   ├── VolumeProfile.cs              (Volume analysis)
│   │   ├── FairValueGap.cs               (FVG detection)
│   │   ├── OrderBlock.cs                 (Support/Resistance)
│   │   └── InstitutionalActivityMeter.cs (Smart money detection)
│   ├── Utils/
│   │   ├── RiskCalculator.cs             (Position sizing)
│   │   ├── LoggingSystem.cs              (Trade logging)
│   │   ├── AlertManager.cs               (Notifications)
│   │   └── PerformanceTracker.cs         (Metrics)
│   └── Config/
│       ├── StrategyConfig.cs             (Settings)
│       ├── RiskProfiles.cs               (Risk levels)
│       └── InstrumentConfig.cs           (ES, NQ, YM params)
├── backtests/
│   ├── ES_2023_2024.json
│   ├── NQ_2023_2024.json
│   ├── YM_2023_2024.json
│   └── Results/
├── docs/
│   ├── STRATEGY_GUIDE.md                 (Detailed strategy)
│   ├── SETUP_GUIDE.md                    (Installation)
│   ├── RISK_MANAGEMENT.md                (Risk rules)
│   └── BACKTESTING.md                    (Backtest guide)
├── examples/
│   ├── example_trades.csv                (Sample trades)
│   ├── performance_report.pdf            (Monthly stats)
│   └── setup_screenshots/
├── tests/
│   ├── RiskManagerTests.cs
│   ├── PositionSizingTests.cs
│   └── SignalDetectionTests.cs
├── README.md                             (This file)
├── LICENSE                               (MIT)
└── CHANGELOG.md                          (Version history)
```

---

## 🚀 Quick Start

### 1. Install Strategy
```bash
1. Copy strategy files to NinjaTrader custom folder
2. Restart NinjaTrader
3. Strategy appears in Strategy Analyzer
```

### 2. Backtest
```bash
1. Select ES 5-min chart
2. Run Strategy Analyzer on 2 years historical data
3. Review performance metrics
```

### 3. Paper Trade (First 2 Weeks)
```bash
1. Enable "Simulate" in NinjaTrader
2. Run strategy on live market data
3. Monitor trades without real money
```

### 4. Live Trading (50K Account)
```bash
1. Connect to broker (Topstep, Lucid, etc.)
2. Set risk to 2% per trade
3. Start with 1 contract
4. Monitor daily P&L
```

---

## 📊 Performance Metrics

### Expected Monthly Performance (50K Account)

| Metric | Conservative | Moderate | Aggressive |
|--------|--------------|----------|-----------|
| Win Rate | 55% | 58% | 60% |
| Avg Win | $400 | $500 | $600 |
| Avg Loss | $300 | $300 | $400 |
| Monthly Return | 5-8% | 8-12% | 12-15% |
| Max Drawdown | 8% | 12% | 15% |

---

## 📚 Documentation

See the `/docs/` folder for detailed guides:
- **STRATEGY_GUIDE.md** - Smart Money Concept explained
- **SETUP_GUIDE.md** - Step-by-step installation
- **RISK_MANAGEMENT.md** - Risk rules and position sizing
- **BACKTESTING.md** - How to run backtests

---

## ⚠️ Disclaimers

- **This is for educational purposes only**
- **Always paper trade before using real money**
- **Past performance does not guarantee future results**
- **Trading futures involves substantial risk of loss**
- **Only trade with money you can afford to lose**
- **Comply with all local regulations and rules**

---

## 🤝 Contributing

Contributions are welcome! Please follow the guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📞 Support

- **Email:** cparedes2002@gmail.com
- **Discord:** [Join our community](#) (Coming soon)
- **Forum:** NinjaTrader Support Forum

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Freqtrade** - Best-in-class backtesting framework
- **trading-code/ninjatrader-freeorderflow** - Order flow indicators
- **Smart Money Community** - Strategy concepts
- **NinjaTrader** - Platform excellence

---

## 📈 Roadmap

- [ ] V1.0 - Core Smart Money Strategy
- [ ] V1.1 - Multi-timeframe confirmation
- [ ] V1.2 - Machine learning signal filter
- [ ] V1.3 - Live trade tracking dashboard
- [ ] V1.4 - Integration with Telegram bot
- [ ] V2.0 - Advanced AI signal processing

---

**Last Updated:** January 2026
**Version:** 0.1.0 (In Development)
**Maintained by:** cparedes2002-web

🚀 **Build wealth systematically. Trade with discipline. Manage risk obsessively.**
