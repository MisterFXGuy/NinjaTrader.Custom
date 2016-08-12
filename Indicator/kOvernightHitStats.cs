#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion
using System.Collections.Generic;
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class kOvernightHitStats : Indicator
    {
        #region Variables

        private int lookbackperiod = 0;
        private Dictionary<string, OvernightStats> dictStats = new Dictionary<string, OvernightStats>();


        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Print("Make sure that the session template contains Overnight data");
            Overlay				= true;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 1))))
                lookbackperiod = 958;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 5))))
                lookbackperiod = 195;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 10))))
                lookbackperiod = 98;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 15))))
                lookbackperiod = 65;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 30))))
                lookbackperiod = 33;

            Print("Using lookback period: " + lookbackperiod);
            Print(string.Format("DATE                MAX            MIN"));

        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            if (BarsPeriod.Id != PeriodType.Minute)
            {
                return;
            }

            if ((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 60)))
            {
                return;
            }

            if (Time[0].TimeOfDay == new TimeSpan(9, 30, 00))
            {
                try
                {
                    int getOvernightBarCount = CurrentBar - lookbackperiod;

                    double maxHigh = MAX(High, lookbackperiod)[0];
                    double minLow = MIN(Low, lookbackperiod)[0];

                    DrawText("txt" + Time[0], string.Format("max: {0} min: {1}", maxHigh.ToString("0.00"), minLow.ToString("0.00")), 0, Low[0] - 4, Color.Black);

                    Print(string.Format("{0}     {1}     {2}",Time[0].ToShortDateString(), maxHigh.ToString("0.00"), minLow.ToString("0.00")));

                    OvernightStats data = new OvernightStats()
                    {
                        High = maxHigh,
                        Low = minLow
                    };

                    dictStats.Add(Time[0].ToShortDateString(), data);
                }
                catch (Exception)
                {

                    Print("Ingnore this bar");
                }


            }
        }

        #region Properties

        #endregion
    }

    public class OvernightStats
    {
        public DateTime Date { get; set; }
        public double High { get; set; }
        public double Low { get; set; }

    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private kOvernightHitStats[] cachekOvernightHitStats = null;

        private static kOvernightHitStats checkkOvernightHitStats = new kOvernightHitStats();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kOvernightHitStats kOvernightHitStats()
        {
            return kOvernightHitStats(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kOvernightHitStats kOvernightHitStats(Data.IDataSeries input)
        {
            if (cachekOvernightHitStats != null)
                for (int idx = 0; idx < cachekOvernightHitStats.Length; idx++)
                    if (cachekOvernightHitStats[idx].EqualsInput(input))
                        return cachekOvernightHitStats[idx];

            lock (checkkOvernightHitStats)
            {
                if (cachekOvernightHitStats != null)
                    for (int idx = 0; idx < cachekOvernightHitStats.Length; idx++)
                        if (cachekOvernightHitStats[idx].EqualsInput(input))
                            return cachekOvernightHitStats[idx];

                kOvernightHitStats indicator = new kOvernightHitStats();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                kOvernightHitStats[] tmp = new kOvernightHitStats[cachekOvernightHitStats == null ? 1 : cachekOvernightHitStats.Length + 1];
                if (cachekOvernightHitStats != null)
                    cachekOvernightHitStats.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cachekOvernightHitStats = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.kOvernightHitStats kOvernightHitStats()
        {
            return _indicator.kOvernightHitStats(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kOvernightHitStats kOvernightHitStats(Data.IDataSeries input)
        {
            return _indicator.kOvernightHitStats(input);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.kOvernightHitStats kOvernightHitStats()
        {
            return _indicator.kOvernightHitStats(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kOvernightHitStats kOvernightHitStats(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.kOvernightHitStats(input);
        }
    }
}
#endregion
