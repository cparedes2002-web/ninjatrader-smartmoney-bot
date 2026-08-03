# Guía de Instalación en NinjaTrader 8

## 📋 Requisitos Previos

✅ NinjaTrader 8 instalado
✅ Visual Studio 2019+ (opcional, pero recomendado)
✅ .NET Framework 4.8+
✅ Archivos .cs descargados

---

## 🚀 PASO 1: Descargar los Archivos

### Opción A: Descargar del repositorio GitHub

1. Ve a: https://github.com/cparedes2002-web/ninjatrader-smartmoney-bot
2. Click en **Code → Download ZIP**
3. Descomprime en `C:\Users\[TuUsuario]\Desktop\`

### Opción B: Clonar con Git

```bash
cd Desktop
git clone https://github.com/cparedes2002-web/ninjatrader-smartmoney-bot.git
cd ninjatrader-smartmoney-bot
```

---

## 📁 PASO 2: Localizar la Carpeta Custom de NinjaTrader

1. Abre **NinjaTrader 8**
2. Ve a **Tools → Import → NinjaScript**
3. Aparecerá una ventana - toma nota de la ruta que muestra
4. La ruta típica es:

```
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\
```

---

## 📂 PASO 3: Copiar Archivos de Estrategias

### Para la ESTRATEGIA PRINCIPAL:

1. Abre tu carpeta descargada: `ninjatrader-smartmoney-bot\src\Strategies`
2. Busca y copia: **`SmartMoneyConceptBot.cs`**
3. Pega en:

```
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Strategies\
```

**Resultado esperado:**
```
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Strategies\SmartMoneyConceptBot.cs
```

---

## 📊 PASO 4: Copiar Archivos de Indicadores

Copia **TODOS** estos archivos desde `ninjatrader-smartmoney-bot\src\Indicators`:

```
✓ OrderBlockIndicator.cs
✓ FairValueGapIndicator.cs
✓ VolumeAnalyzer.cs
✓ InstitutionalActivityMeter.cs
```

Pega TODOS en:

```
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Indicators\
```

**Resultado esperado:**
```
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Indicators\OrderBlockIndicator.cs
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Indicators\FairValueGapIndicator.cs
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Indicators\VolumeAnalyzer.cs
C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\Indicators\InstitutionalActivityMeter.cs
```

---

## 🔧 PASO 5: Reiniciar NinjaTrader

1. **Cierra completamente NinjaTrader** (incluye bandeja del sistema)
2. Espera 10 segundos
3. **Abre NinjaTrader nuevamente**
4. El software compilará automáticamente los archivos

**Indicadores que debería ver compilando:**
```
Compiling OrderBlockIndicator...
Compiling FairValueGapIndicator...
Compiling VolumeAnalyzer...
Compiling InstitutionalActivityMeter...
Compiling SmartMoneyConceptBot strategy...
```

---

## ✅ PASO 6: Verificar que Compiló Correctamente

### Opción A: Verificar en NinjaTrader

1. Abre NinjaTrader
2. Ve a **Tools → Edit NinjaScript → Indicators**
3. Deberías ver en la lista:
   - ✅ OrderBlockIndicator
   - ✅ FairValueGapIndicator
   - ✅ VolumeAnalyzer
   - ✅ InstitutionalActivityMeter

4. Ve a **Tools → Edit NinjaScript → Strategies**
5. Deberías ver:
   - ✅ SmartMoneyConceptBot

### Opción B: Verificar en Output Window

1. Si hay ERRORES, aparecerán en **Tools → Output Window**
2. Típicos errores:

```
Error CS1002: ; expected
→ Falta punto y coma en el código

Error CS0103: The name 'XXX' does not exist
→ Falta importar una librería (using declaration)

Error CS0246: The type 'XXX' could not be found
→ Librería no correctamente importada
```

---

## 🎯 PASO 7: Probar la Estrategia

### Test en Chart:

1. Abre un gráfico de **ES (E-mini S&P 500)** 5-minutos
2. Ve a **Tools → Strategy Analyzer**
3. Selecciona **SmartMoneyConceptBot**
4. Dale PLAY para paper trading
5. Deberías ver:
   - Señales de entrada (BUY/SELL)
   - Líneas de SL y TP en el chart
   - Logs en la ventana Output

---

## 🔍 PASO 8: Verificar Indicadores en Chart

1. Abre gráfico de **ES 5-min**
2. Ve a **Insert → Indicator → OrderBlockIndicator**
3. Verifica que aparezca en el chart
4. Repite para otros indicadores:
   - FairValueGapIndicator
   - VolumeAnalyzer
   - InstitutionalActivityMeter

---

## ❌ TROUBLESHOOTING (Si Hay Errores)

### Error 1: "Compilation failed"

**Solución:**
1. Cierra NinjaTrader completamente
2. Ve a: `C:\Users\[TuUsuario]\Documents\NinjaTrader 8\bin\Custom\`
3. Busca archivo `.log` con el error específico
4. Lee el error y verifica el archivo .cs
5. Reinicia NinjaTrader

### Error 2: "Indicators don't appear in Tools menu"

**Solución:**
1. Verifica que archivo esté en carpeta correcta
2. Nombre del archivo debe coincidir con nombre de clase
3. Ej: `public class OrderBlockIndicator` → `OrderBlockIndicator.cs`
4. Reinicia NinjaTrader

### Error 3: "Strategy won't start"

**Solución:**
1. Verifica que todos los indicadores compilaron
2. Ve a Output Window y busca errores
3. Puede ser que falte algún indicador
4. Verifica que la estrategia importa los indicadores correctamente

---

## 📊 PASO 9: Backtesting en Strategy Analyzer

Una vez que todo compile correctamente:

1. **Tools → Strategy Analyzer**
2. Selecciona **SmartMoneyConceptBot**
3. Instrumento: **ES** (o NQ, YM)
4. Timeframe: **5 Minute**
5. Rango de fechas: **2+ años** (ej: 2022-2024)
6. Click **Run**
7. Espera a que termine (5-15 minutos)

**Resultado esperado:**
```
Total Trades: 150-300
Win Rate: 55-65%
Profit Factor: 1.5-2.2
Max Drawdown: 10-20%
```

---

## 🚀 PASO 10: Configurar Parámetros

Antes de paper trading, personaliza:

1. Abre **Tools → Strategy Analyzer**
2. Selecciona **SmartMoneyConceptBot**
3. Modifica estos parámetros:

```
Account Balance: 50000
Risk Per Trade: 2.0%
Daily Drawdown Limit: 5.0%
Max Position Size: 2 (contracts)
Stop Loss Points: 20
Take Profit Ratio: 2.0
```

4. Click **Run** para ver impacto en performance

---

## 📝 PASO 11: Paper Trading

Antes de dinero REAL:

1. Conecta tu broker en NinjaTrader (Paper/Simulated account)
2. Ve a **Tools → Strategy Analyzer**
3. Selecciona **SmartMoneyConceptBot**
4. Click **ON** (no RUN, sino ON para live)
5. Monitorea trades por **2-4 semanas**
6. Si consistentemente rentable → Dinero real

---

## 💼 PASO 12: Dinero Real (Cuando estés listo)

Solo después de:
- ✅ Backtest positivo
- ✅ Paper trading rentable
- ✅ 2+ semanas sin pérdidas
- ✅ Comprendes la estrategia

Entonces:
1. Conecta cuenta REAL en NinjaTrader
2. Empieza con **1 contrato MÁXIMO**
3. Monitorea diariamente
4. Después de 20+ trades rentables → Aumenta a 2 contratos

---

## 📞 SOPORTE

Si tienes errores de compilación:

1. Copia el mensaje de error exacto
2. Verifica nombre del archivo vs nombre de clase
3. Revisa que uses NinjaTrader 8, no versión anterior
4. Intenta eliminar archivos y copiar de nuevo
5. Si persiste: GitHub Issues → cparedes2002-web/ninjatrader-smartmoney-bot

---

## ✅ CHECKLIST FINAL

Antes de trader con dinero real, verifica:

```
☐ Todos los archivos .cs copiados
☐ NinjaTrader reiniciado
☐ Indicadores aparecen en Tools menu
☐ Estrategia aparece en Strategy Analyzer
☐ Backtest corre sin errores
☐ Backtest muestra win rate > 55%
☐ Paper trading por 2+ semanas
☐ Journal de trades revisado
☐ Risk management entendido
☐ Dinero disponible que puedes perder
```

---

**¡Felicidades! Ya tienes instalado el Smart Money Concept Bot en NinjaTrader.**

🚀 **Ahora es momento de practicar con disciplina y paciencia.**
