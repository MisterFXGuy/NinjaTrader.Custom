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
using MySql.Data.MySqlClient;
using System.Collections.Generic;

#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class kEventOnChart : Indicator
    {
        #region Variables
         private MySqlConnection dbConn = new MySqlConnection("server = localhost; database=algo;uid=root;password=Password1;");
         private string query = "select* from economiceventdata where datenum between '2016-04-01' and '2016-09-01' and event <> '  Crude Oil Inventories  ' order by datenum asc";
         private Dictionary<DateTime,List<EconomicData>> timeClusteredEvents = new Dictionary<DateTime, List<EconomicData>>();
         private Dictionary<DateTime, List<EconomicData>> timeClusteredEventsHourly = new Dictionary<DateTime, List<EconomicData>>();
         private Dictionary<DateTime, List<EconomicData>> timeClusteredEventDaily = new Dictionary<DateTime, List<EconomicData>>();

        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Overlay				= true;
        }

        protected override void OnStartUp()
        {
            dbConn.Open();
            using (MySqlCommand command = new MySqlCommand(query, dbConn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        if (reader.GetString(2).Contains("PM") || reader.GetString(2).Contains("AM"))
                        {
                            
                            DateTime sqlTime = DateTime.Parse(reader.GetString(1) + " " + reader.GetString(2));
                            DateTime subtractTime = sqlTime.AddHours(-1);
                            DateTime modifiedTime = (TimeZoneInfo.Local.IsDaylightSavingTime(sqlTime) == true) ? sqlTime : subtractTime;

                            EconomicData data = new EconomicData()
                            {
                                Id = reader.GetUInt32(0),
                                Timestamp = modifiedTime,
                                Currency = reader.GetString(3),
                                Impact = reader.GetString(4),
                                Event = reader.GetString(5),
                                Actual = reader.GetString(6),
                                Forecast = reader.GetString(7),
                                Previous = reader.GetString(8)
                            };

                            if (!timeClusteredEvents.ContainsKey(modifiedTime)) //if it does not contain date
                            {
                                List<EconomicData> eventData = new List<EconomicData>();
                                eventData.Add(data);
                                timeClusteredEvents.Add(modifiedTime, eventData);

                            }
                            else //if it contains the key, append the data
                            {
                                timeClusteredEvents[modifiedTime].Add(data);
                            }
                        }
                    }
                }
            }
            dbConn.Close();

            foreach (KeyValuePair<DateTime, List<EconomicData>> valuePair in timeClusteredEvents)
            {
                //TO DO: I need a way to get the data on an hourly and daily basis so that I can see a smaller chart with all the data and pivot point
                // I would also like to create a cumm volume profile and on bar value area. 
            }       
        }

        protected override void OnBarUpdate()
        {
            if (((BarsPeriod.Id == PeriodType.Minute) &&
                 ((BarsPeriod.Value == 1) || (BarsPeriod.Value == 5) || (BarsPeriod.Value == 10) ||
                  (BarsPeriod.Value == 15) || (BarsPeriod.Value == 30))))
            {
                if (timeClusteredEvents.ContainsKey(Time[0]) == true)
                {
                    string drawTextContent = "";
                    List<DateTime> markDate = new List<DateTime>();


                    foreach (EconomicData element in timeClusteredEvents[Time[0]])
                    {
                        if (element.Currency == "USD" && (element.Impact == "High"))
                        {

                            if (element.Actual != "" || element.Forecast != "" || element.Previous != "")
                            {
                                drawTextContent = drawTextContent +
                                                  string.Format("{0}- {1} {2}", element.Event, element.Id,
                                                      Environment.NewLine);
                                markDate.Add(Time[0]);
                            }
                        }
                    }


                    if (markDate.Contains(Time[0]))
                    {
                        BackColor = Color.LightCyan;
                        DrawText("stat" + Time[0], drawTextContent, 0, Low[0] - 10,
                            Color.Black);
                    }

                }
            }
            if (((BarsPeriod.Id == PeriodType.Minute) && ((BarsPeriod.Value == 60))))
            {
                // TO DO: Work on charting the data on a hourly and montly basis. 
            }




        }
        static DateTime RoundToHour(DateTime dt)
        {
            long ticks = dt.Ticks + 18000000000;
            return new DateTime(ticks - ticks % 36000000000, dt.Kind);
        }

        #region Properties

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private kEventOnChart[] cachekEventOnChart = null;

        private static kEventOnChart checkkEventOnChart = new kEventOnChart();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kEventOnChart kEventOnChart()
        {
            return kEventOnChart(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kEventOnChart kEventOnChart(Data.IDataSeries input)
        {
            if (cachekEventOnChart != null)
                for (int idx = 0; idx < cachekEventOnChart.Length; idx++)
                    if (cachekEventOnChart[idx].EqualsInput(input))
                        return cachekEventOnChart[idx];

            lock (checkkEventOnChart)
            {
                if (cachekEventOnChart != null)
                    for (int idx = 0; idx < cachekEventOnChart.Length; idx++)
                        if (cachekEventOnChart[idx].EqualsInput(input))
                            return cachekEventOnChart[idx];

                kEventOnChart indicator = new kEventOnChart();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                kEventOnChart[] tmp = new kEventOnChart[cachekEventOnChart == null ? 1 : cachekEventOnChart.Length + 1];
                if (cachekEventOnChart != null)
                    cachekEventOnChart.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cachekEventOnChart = tmp;
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
        public Indicator.kEventOnChart kEventOnChart()
        {
            return _indicator.kEventOnChart(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kEventOnChart kEventOnChart(Data.IDataSeries input)
        {
            return _indicator.kEventOnChart(input);
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
        public Indicator.kEventOnChart kEventOnChart()
        {
            return _indicator.kEventOnChart(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kEventOnChart kEventOnChart(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.kEventOnChart(input);
        }
    }
}
#endregion
