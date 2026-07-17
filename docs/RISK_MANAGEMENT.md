# Risk Management Guide

## Core Principle

**"It's not how much you make, it's how much you don't lose that matters."**

Risk management is the PRIMARY focus of professional traders. Your biggest losses will determine your success more than your biggest wins.

## Position Sizing Formula

### Fixed Fractional Method (Recommended)

```
Position Size = (Account × Risk%) / (Stop Loss Points × Point Value)
```

**Example (50K ES Account):**
```
Account: $50,000
Risk %: 2%
Stop Loss: 20 points
Point Value (ES): $50

Calculation:
(50,000 × 2%) / (20 × 50) = 1,000 / 1,000 = 1 contract
```

### Kelly Criterion (Advanced)

```
Kelly % = (Win% × Avg Win) - (Loss% × Avg Loss) / Avg Win

Fractional Kelly = Kelly % × 0.25 (conservative)
```

**Example:**
```
Win Rate: 60%
Avg Win: $500
Avg Loss: $300

Kelly % = (0.60 × 500) - (0.40 × 300) / 500
Kelly % = (300 - 120) / 500 = 36%
Fractional Kelly (25%) = 9% per trade
```

## Account Allocation by Risk Level

### Conservative (Beginner)
```
Risk per trade: 1% of account
Daily max loss: 3% of account
Monthly max loss: 10% of account
Position size (50K, 20pt SL): 0.5 contracts
```

### Moderate (Experienced)
```
Risk per trade: 2% of account
Daily max loss: 5% of account
Monthly max loss: 15% of account
Position size (50K, 20pt SL): 1 contract
```

### Aggressive (Professional)
```
Risk per trade: 3% of account
Daily max loss: 7% of account
Monthly max loss: 20% of account
Position size (50K, 20pt SL): 1.5 contracts
```

## Daily Loss Limits

### Implementation

```python
# Check at start of each trading day
if daily_loss >= max_daily_loss:
    STOP_TRADING_FOR_DAY = True
    
# Example
50K account × 5% = $2,500 max daily loss
Once account drops $2,500 → STOP
```

### Psychology

Daily loss limits prevent:
- **Chase Trading:** Trying to recover losses
- **Revenge Trading:** Emotional decision-making
- **Overleverage:** Desperation bets
- **Catastrophic Losses:** Account blowup

## Stop Loss Management

### Initial Stop Loss Placement

```
BULLISH TRADE:
Entry: 4,751.00
Stop Loss: 4,741.00 (10 points below support/order block)
Risk: $500 (10 points × $50)

BEARISH TRADE:
Entry: 16,195.00
Stop Loss: 16,205.00 (10 points above resistance/order block)
Risk: $500 (10 points × $50)
```

### Trailing Stop Strategy

```
Once trade reaches 1:1 Risk/Reward:
- Move stop loss to breakeven
- Lock in 0 loss if trade reverses

Once trade reaches 2:1 Risk/Reward:
- Trail stop by 5 points
- Protect 75% of potential profits
```

## Profit Targets (Tiered Exit)

### Standard 3-Tier Exit

```
Position: 1 contract = 100 shares equivalent

TP1: 0.5 position at 1:1 RR
- Exit 50% of position
- Lock in $500 profit
- Risk remaining 50% to trail

TP2: 0.3 position at 1.5:1 RR
- Exit 30% of position
- Lock in additional $250 profit
- Trail remaining 20%

TP3: 0.2 position at 2:1 RR+
- Trail last 20% for maximum profit
- Let winner run
```

### Example Trade Execution

```
Entry: ES 4,751.00 (1 contract)
Stop Loss: 4,741.00 (20 point risk = $1,000)

TP1 (1:1 RR): 4,761.00
- Exit 50 shares
- Profit: $500
- Remaining: 50 shares at breakeven

TP2 (1.5:1 RR): 4,766.00
- Exit 30 shares
- Profit: $250
- Remaining: 20 shares

TP3 (2:1 RR): 4,771.00
- Exit 20 shares
- Profit: $200
Total Profit: $950
```

## Account Drawdown Limits

### Pause Levels

```
10% Drawdown: Review strategy, check journal
15% Drawdown: Reduce position size to 50%
20% Drawdown: Reduce position size to 25%
25% Drawdown: Stop trading, reassess
```

### Recovery Plan

```
After 15% Drawdown:
1. STOP live trading
2. Paper trade for 1 week
3. Review last 20 trades
4. Identify if:
   - Strategy failed
   - Execution failed
   - Market changed
5. Adjust and restart
```

## Instrument-Specific Parameters

### ES (E-mini S&P 500)
```
Tick Size: 0.25 points
Point Value: $50
Liquid Hours: 9:30 AM - 4:00 PM ET
Avg Spread: 1-2 points
Min Stop Loss: 10 points
Recommended SL: 15-20 points
```

### NQ (E-mini NASDAQ)
```
Tick Size: 0.25 points
Point Value: $20
Liquid Hours: 9:30 AM - 4:00 PM ET
Avg Spread: 2-3 points
Min Stop Loss: 15 points
Recommended SL: 20-25 points
```

### YM (E-mini Dow)
```
Tick Size: 1 point
Point Value: $5
Liquid Hours: 9:30 AM - 4:00 PM ET
Avg Spread: 3-5 points
Min Stop Loss: 20 points
Recommended SL: 25-30 points
```

## Risk Metrics to Track

### Essential Metrics

```
1. Win Rate = (Winning Trades / Total Trades) × 100
   Target: 55%+

2. Profit Factor = Total Wins / Total Losses
   Target: 1.5+

3. Average Win / Average Loss Ratio
   Target: 1.5:1+

4. Maximum Drawdown = Peak to Trough decline
   Target: < 20%

5. Sharpe Ratio = (Return - Risk-Free) / StdDev
   Target: > 1.0

6. Risk/Reward per Trade
   Target: > 1:1.5
```

## Pre-Trade Checklist

Before EVERY trade, verify:

```
☑ Risk amount correct? (2% of account max)
☑ Position size calculated? (Using formula)
☑ Stop loss set? (Below order block/support)
☑ Take profit set? (2:1 RR minimum)
☑ Volume confirmed? (1.5x average+)
☑ Timeframe aligned? (HTF confirms direction)
☑ Daily loss limit? (Not exceeded yet)
☑ Market hours? (Liquid trading hours)
☑ News events? (No major releases)
☑ Technical setup valid? (Order block + FVG)
```

## Monthly Review

### What to Track

```
1. Total Profit/Loss
2. Number of trades
3. Win rate
4. Profit factor
5. Average win size
6. Average loss size
7. Largest win
8. Largest loss
9. Days with losses > $500
10. Best trading day
11. Worst trading day
12. Average daily P&L
```

### Analysis Questions

```
1. Were losses from SL hits (system failure) or exits (discipline failure)?
2. Which setups performed best? Worst?
3. What time of day was most profitable?
4. Did the 2% per trade rule increase profits?
5. Were emotions a factor in losses?
6. Should I adjust position sizing?
7. Which instruments traded best?
```

## Account Growth Projection

### Conservative (1% Risk, 55% Win Rate)

```
Monthly Return: 4-6%
Year 1: $50,000 → $60,000-70,000
Year 2: $70,000 → $95,000-120,000
Year 3: $120,000 → $200,000+
```

### Moderate (2% Risk, 58% Win Rate)

```
Monthly Return: 8-12%
Year 1: $50,000 → $80,000-100,000
Year 2: $100,000 → $180,000-220,000
Year 3: $220,000 → $500,000+
```

## Summary: Risk Management Rules

1. **Always use stops** - Non-negotiable
2. **Risk only 2% per trade** - Protects account
3. **Max 5% daily loss** - Prevents catastrophe
4. **Use tiered exits** - Locks profits
5. **Scale position with win rate** - Mathematical edge
6. **Track metrics** - Data-driven decisions
7. **Review monthly** - Continuous improvement
8. **Paper trade first** - Validate strategy
9. **Never revenge trade** - Emotional death
10. **Preserve capital** - First rule of wealth

---

**Remember:** The goal isn't to win every trade. The goal is to survive long enough to win overall.
