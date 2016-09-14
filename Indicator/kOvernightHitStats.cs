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
using MySql.Data.MySqlClient;
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
        private MySqlConnection dbConn = new MySqlConnection("server = localhost; database=algo;uid=root;password=Password1;");
        private int onLookbackPeriod = 0;
        private int rthLookbackPeriod = 0;
        private int ibLookbackPeriod = 0;

        private Dictionary<string, SessionStats> distOvernightStats = new Dictionary<string, SessionStats>();
        private Dictionary<string, SessionStats> distRTHStats = new Dictionary<string, SessionStats>();
        private Dictionary<string, SessionStats> distInitialBalance = new Dictionary<string, SessionStats>();

        private List<string> listDateSaved = new List<string>(); 
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Print("Make sure that the session template contains Overnight data");
            Overlay				= true;

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 1))))
            {
                onLookbackPeriod = 958;
                rthLookbackPeriod = 390;
                ibLookbackPeriod = 60;
            }


            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 5))))
            {
                onLookbackPeriod = 195;
                rthLookbackPeriod = 78;
                ibLookbackPeriod = 12;

            }


            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 10))))
            {
                onLookbackPeriod = 98;
                rthLookbackPeriod = 39;
                ibLookbackPeriod = 6;
            }


            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 15))))
            {
                onLookbackPeriod = 65;
                rthLookbackPeriod = 26;
                ibLookbackPeriod = 4;
            }

            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 30))))
            {
                onLookbackPeriod = 33;
                rthLookbackPeriod = 13;
                ibLookbackPeriod = 2;
            }
                
            
            Print("Using overnight lookback period: " + onLookbackPeriod + " and rth lookback period: " + rthLookbackPeriod);

        }

        protected override void OnStartUp()
        {
            string queryDate = "select datenum from sessionstats order by datenum asc";
            dbConn.Open();
            using (MySqlCommand command = new MySqlCommand(queryDate, dbConn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listDateSaved.Add(reader.GetString(0));
                    }
                }

            }

            dbConn.Close();
        }

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
                    int getOvernightBarCount = CurrentBar - onLookbackPeriod;

                    //DrawText("txt" + Time[0], string.Format("max: {0} min: {1}  {2}", maxHigh.ToString("0.00"), minLow.ToString("0.00"),CurrentBar.ToString()), 0, Low[0] - 4, Color.Black);
                    //Print("ON Lookback times:" + Time[onLookbackPeriod] + "    " + Time[0]);
                    //Print(string.Format("{0}     {1}     {2}",Time[0].ToShortDateString(), maxHigh.ToString("0.00"), minLow.ToString("0.00")));

                    SessionStats data = new SessionStats()
                    {
                        LookbackTime = Time[onLookbackPeriod],
                        Time = Time[0],
                        Open = Open[onLookbackPeriod],
                        High = MAX(High, onLookbackPeriod)[0],
                        Low = MIN(Low, onLookbackPeriod)[0],
                        Close = Close[0]
                    };

                    DrawText("c" + Time[0], string.Format("max: {0} {3} min: {1}  {3} {2}", data.High.ToString("0.00"), data.Low.ToString("0.00"), "", Environment.NewLine), Convert.ToInt32(onLookbackPeriod/2), data.Low - 1, Color.Black);

                    distOvernightStats.Add(Time[0].ToShortDateString(), data);
                }
                catch (Exception)
                {Print("There is an issue with generating the first day due to lookback period...skip");}
            }

            if (Time[0].TimeOfDay == new TimeSpan(10, 30, 00))
            {

                int getOvernightBarCount = CurrentBar - ibLookbackPeriod;

                SessionStats data = new SessionStats()
                {
                    LookbackTime = Time[ibLookbackPeriod],
                    Time = Time[0],
                    Open = Open[ibLookbackPeriod],
                    High = MAX(High, ibLookbackPeriod)[0],
                    Low = MIN(Low, ibLookbackPeriod)[0],
                    Close = Close[0]

                };

                if (distOvernightStats.ContainsKey(Time[0].ToShortDateString()) == true)
                    distInitialBalance.Add(Time[0].ToShortDateString(), data);

                //Print("IB Lookback times:" + Time[ibLookbackPeriod] + "    " + Time[0]);

                string hitString = "";
                if (distOvernightStats.ContainsKey(Time[0].ToShortDateString()) == true)
                {
                    if (distInitialBalance[Time[0].ToShortDateString()].High <
                        distOvernightStats[Time[0].ToShortDateString()].High &&
                        distInitialBalance[Time[0].ToShortDateString()].Low >
                        distOvernightStats[Time[0].ToShortDateString()].Low)
                        hitString = "MISS";
                    else if (distInitialBalance[Time[0].ToShortDateString()].High <
                             distOvernightStats[Time[0].ToShortDateString()].Low)
                        hitString = "MISS";
                    else if (distInitialBalance[Time[0].ToShortDateString()].Low >
                             distOvernightStats[Time[0].ToShortDateString()].High)
                        hitString = "MISS";
                    else
                        hitString = "HIT";

                    distInitialBalance[Time[0].ToShortDateString()].Hit = hitString;
                    DrawText("b" + Time[0], string.Format("max: {0} {3} min: {1}  {3} {2} {4}", data.High.ToString("0.00"), data.Low.ToString("0.00"), "", Environment.NewLine, hitString), Convert.ToInt32(ibLookbackPeriod / 2), data.Low - 1, Color.Black);
                }



               // DrawText("a" + Time[0], string.Format("max: {0} {3} min: {1}  {3} {2}", data.High.ToString("0.00"), data.Low.ToString("0.00"), "", Environment.NewLine), Convert.ToInt32(ibLookbackPeriod / 2), data.Low - 1, Color.Black);
            }

            if (Time[0].TimeOfDay == new TimeSpan(16, 00, 00))
            {
                try
                {
                    int getOvernightBarCount = CurrentBar - rthLookbackPeriod;

                    //Print("close Lookback times:" + Time[rthLookbackPeriod] + "    " + Time[0]);
                   

                    SessionStats data = new SessionStats()
                    {
                        LookbackTime = Time[rthLookbackPeriod],
                        Time = Time[0],
                        Open = Open[rthLookbackPeriod],
                        High = MAX(High, rthLookbackPeriod)[0],
                        Low = MIN(Low, rthLookbackPeriod)[0],
                        Close = Close[0]

                    };
                    

                    if (distOvernightStats.ContainsKey(Time[0].ToShortDateString()) == true)
                        distRTHStats.Add(Time[0].ToShortDateString(), data);

                    string hitString = "";

                    if (distOvernightStats.ContainsKey(Time[0].ToShortDateString()) == true)
                    {
                        if (distRTHStats[Time[0].ToShortDateString()].High <
                            distOvernightStats[Time[0].ToShortDateString()].High &&
                            distRTHStats[Time[0].ToShortDateString()].Low >
                            distOvernightStats[Time[0].ToShortDateString()].Low)
                            hitString = "MISS";
                        else if (distRTHStats[Time[0].ToShortDateString()].High <
                                 distOvernightStats[Time[0].ToShortDateString()].Low)
                            hitString = "MISS";
                        else if (distRTHStats[Time[0].ToShortDateString()].Low >
                                 distOvernightStats[Time[0].ToShortDateString()].High)
                            hitString = "MISS";
                        else
                            hitString = "HIT";

                        distRTHStats[Time[0].ToShortDateString()].Hit = hitString;

                        DrawLine("overnighHigh" + Time[0].Date, false, distOvernightStats[Time[0].ToShortDateString()].LookbackTime, distOvernightStats[Time[0].ToShortDateString()].High, distOvernightStats[Time[0].ToShortDateString()].Time, distOvernightStats[Time[0].ToShortDateString()].High, Color.Aqua, DashStyle.Solid, 1);
                        DrawLine("overnighLow" + Time[0].Date, false, distOvernightStats[Time[0].ToShortDateString()].LookbackTime, distOvernightStats[Time[0].ToShortDateString()].Low, distOvernightStats[Time[0].ToShortDateString()].Time, distOvernightStats[Time[0].ToShortDateString()].Low, Color.Aqua, DashStyle.Solid, 1);

                        DrawLine("ibHigh" + Time[0].Date, false, distInitialBalance[Time[0].ToShortDateString()].LookbackTime, distInitialBalance[Time[0].ToShortDateString()].High, distInitialBalance[Time[0].ToShortDateString()].Time, distInitialBalance[Time[0].ToShortDateString()].High, Color.Orange, DashStyle.Solid, 1);
                        DrawLine("ibLow" + Time[0].Date, false, distInitialBalance[Time[0].ToShortDateString()].LookbackTime, distInitialBalance[Time[0].ToShortDateString()].Low, distInitialBalance[Time[0].ToShortDateString()].Time, distInitialBalance[Time[0].ToShortDateString()].Low, Color.Orange, DashStyle.Solid, 1);

                        DrawLine("rthHigh" + Time[0].Date, false, distRTHStats[Time[0].ToShortDateString()].LookbackTime, distRTHStats[Time[0].ToShortDateString()].High, distRTHStats[Time[0].ToShortDateString()].Time, distRTHStats[Time[0].ToShortDateString()].High, Color.ForestGreen, DashStyle.Solid, 1);
                        DrawLine("rthlow" + Time[0].Date, false, distRTHStats[Time[0].ToShortDateString()].LookbackTime, distRTHStats[Time[0].ToShortDateString()].Low, distRTHStats[Time[0].ToShortDateString()].Time, distRTHStats[Time[0].ToShortDateString()].Low, Color.ForestGreen, DashStyle.Solid, 1);

                        DrawText("b" + Time[0], string.Format("max: {0} {3} min: {1}  {3} {2} {4}", data.High.ToString("0.00"), data.Low.ToString("0.00"), "", Environment.NewLine, hitString), Convert.ToInt32(rthLookbackPeriod / 2), data.Low - 1, Color.Black);
                    }


                    //Print(string.Format("{0}     {1}     {2}  {3}", "                   - ", data.High.ToString("0.00"), data.Low.ToString("0.00"),hitString));


                    SaveDataToDatabase(distOvernightStats[Time[0].ToShortDateString()], distInitialBalance[Time[0].ToShortDateString()], distRTHStats[Time[0].ToShortDateString()]);

                }
                catch (Exception)
                { Print("There is an issue with generating the eod day due to lookback period...skip");}
            }

            //string insertQuery = "INSERT INTO sessionstats (dateNum, on_open, on_high, on_low, on_close, ib_open, ib_high, ib_low, ib_close, day_open, day_high, day_low, day_close)";
        }
        private void SaveDataToDatabase(SessionStats overnight, SessionStats initialBalance, SessionStats day)
        {
            if (!listDateSaved.Contains(overnight.Time.ToShortDateString()))
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = dbConn;
                dbConn.Open();

                cmd.CommandText =
                    "Insert into sessionstats values (@dateNum, @on_open,@on_high,@on_low,@on_low,@ib_open,@ib_high,@ib_low,@ib_close,@day_open,@day_high,@day_low,@day_close,@ib_hit,@day_hit)";
                cmd.Parameters.Add(new MySqlParameter("@dateNum", overnight.Time.ToShortDateString()));
                cmd.Parameters.Add(new MySqlParameter("@on_open", overnight.Open));
                cmd.Parameters.Add(new MySqlParameter("@on_high", overnight.High));
                cmd.Parameters.Add(new MySqlParameter("@on_low", overnight.Low));
                cmd.Parameters.Add(new MySqlParameter("@on_close", overnight.Close));
                cmd.Parameters.Add(new MySqlParameter("@ib_open", initialBalance.Open));
                cmd.Parameters.Add(new MySqlParameter("@ib_high", initialBalance.High));
                cmd.Parameters.Add(new MySqlParameter("@ib_low", initialBalance.Low));
                cmd.Parameters.Add(new MySqlParameter("@ib_close", initialBalance.Close));
                cmd.Parameters.Add(new MySqlParameter("@day_open", day.Open));
                cmd.Parameters.Add(new MySqlParameter("@day_high", day.High));
                cmd.Parameters.Add(new MySqlParameter("@day_low", day.Low));
                cmd.Parameters.Add(new MySqlParameter("@day_close", day.Close));
                cmd.Parameters.Add(new MySqlParameter("@ib_hit", initialBalance.Hit));
                cmd.Parameters.Add(new MySqlParameter("@day_hit", day.Hit));
                cmd.ExecuteNonQuery();
                dbConn.Close();
                listDateSaved.Remove(overnight.Time.ToShortDateString());
            }
        }

        #region Properties

        #endregion
    }

    public class SessionStats
    {
        public DateTime LookbackTime { get; set; }
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public string Hit { get; set; }

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
