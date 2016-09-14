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
using System.Linq;

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class kOvernightExtremeTest : Indicator
    {
        #region Variables
        TimeSpan RTHStartTime = new TimeSpan(01,09, 30, 00);
        TimeSpan RTHEndTime = new TimeSpan(01,16, 00, 00);
        TimeSpan IBEndTime = new TimeSpan(01,10, 30, 00);
        DateTime CurrentStartTime = new DateTime();
        DateTime UpcomingRTHStartTime = new DateTime();
        DateTime UpcomingIBEndTime = new DateTime();
        DateTime UpcomingRTHEndTime = new DateTime();

        private Dictionary<double, int> dictPriceStack = new Dictionary<double, int>();
        private Dictionary<string,PriceStack>  distONPriceStack = new Dictionary<string, PriceStack>();
        private Dictionary<string, PriceStack> distIBPriceStack = new Dictionary<string, PriceStack>();
        private Dictionary<string, PriceStack> distRTHPriceStack = new Dictionary<string, PriceStack>();

        

        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Overlay				= true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Asssign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
            if (CurrentBar == 0)
            {
                CurrentStartTime = Time[0];
                
                UpcomingRTHStartTime = (CurrentStartTime.Date + RTHStartTime);
                UpcomingIBEndTime = (CurrentStartTime.Date + IBEndTime);
                UpcomingRTHEndTime = (CurrentStartTime.Date + RTHEndTime);

                double? onHigh = null;
                double? onLow = null;
                double? ibHigh = null;
                double? ibLow = null;
                double? rthHigh = null;
                double? rthLow = null;

                DateTime? onHighTime = null;
                DateTime? onLowTime = null;
                DateTime? ibHighTime = null;
                DateTime? ibLowTime = null;
                DateTime? rthHighTime = null;
                DateTime? rthLowTime = null;


                //Print(CurrentStartTime);ss
                //Print(string.Format("start {0} open {1} ib {2}  end {3}", CurrentStartTime, UpcomingRTHStartTime, UpcomingIBEndTime, UpcomingRTHEndTime));
            }

            if (Time[0] < UpcomingRTHStartTime)
            {
                // DO WORK LEADING UP TO RTH SESSION
               // Print("overnights Period " + Time[0]);
               //Print("Test");
               //RecordPriceStack(distONPriceStack,Time[0]+"."+CurrentBar,Close[0],Volume[0]);

            }
            else if (UpcomingRTHStartTime <= Time[0] && Time[0] < UpcomingIBEndTime)
            {
                //DO WORK DURING IB PERIOD
                //Print("IB Period " + Time[0]);

                //RecordPriceStack(distIBPriceStack, Time[0] + "." + CurrentBar, Close[0], Volume[0]);
                //
            }
            else if (UpcomingIBEndTime <= Time[0] && Time[0] < UpcomingRTHEndTime)
            {
                //DO WORK TIL RTH SESSION END
                //Print("rth period " + Time[0]);
                RecordPriceStack(distRTHPriceStack, Time[0] + "." + CurrentBar, Close[0], Volume[0]);

            }
            else
            {

                //DO WORK WHEN RTH SESSION ENTERS OVERNIGHT

                UpcomingRTHStartTime = (UpcomingRTHStartTime.Date + RTHStartTime);
                UpcomingIBEndTime = (UpcomingIBEndTime.Date + IBEndTime);
                UpcomingRTHEndTime = (UpcomingRTHEndTime.Date + RTHEndTime);

                //Print(string.Format("ibOpen {0}   ibClose {1}   eod {2}", UpcomingRTHStartTime, UpcomingIBEndTime, UpcomingRTHEndTime));
                GenerateSlopeOfPriceStackAndCummalativeVolumeProfile(distRTHPriceStack);
                distRTHPriceStack.Clear();



            }





}

        private void GenerateEstimatedPriceStack(double price, int volume)
        {
            if (dictPriceStack.ContainsKey(price))
            {
                //Add the volume of the var to existing price stack to generate an estimated cummalitive average
                //It is estiamated because the var itself on the close could have multiple price. I don't know how to record tick volume yet
                dictPriceStack[price] = dictPriceStack[price] + volume;
            }
            else
            {
                dictPriceStack.Add(price, volume);
            }
        }

        private void RecordPriceStack(Dictionary<string, PriceStack> dict, string BarId, double price, double volume)
        {
            //Print(String.Format("{0}  {1}  {2}",BarId,price,volume));
            PriceStack obj = new PriceStack()
            {
                Time = BarId,
                Price = price,
                Volume = volume
            };

            if (dict.ContainsKey(BarId)==true)
            {
                Print("Dictionary contains key, there is a possible collision..");
            }
            else
            {
                dict.Add(BarId,obj);
            }
        }

        private void GenerateSlopeOfPriceStackAndCummalativeVolumeProfile(Dictionary<string, PriceStack> obj)
        {
            double[] slopeStats = new double[3];
            SortedDictionary<double, double> cummVolumeStats = new SortedDictionary<double, double>();

            List<double> prices = new List<double>();

            foreach (KeyValuePair<string, PriceStack> data in obj)
            {
                //Print(String.Format("{0}  {1}  {2}", data.Value.Time, data.Value.Price, data.Value.Volume));
                prices.Add(data.Value.Price); 
            }

            //prices.ToArray();
            int[] xUniformInts = Enumerable.Range(0, prices.Count()).ToArray();

            //Print("Calculate the Slope");
            //GET LIST OF ALL STATISTICAL DAtA FOR THIS CHART. 


            cummVolumeStats = GetCummalitivePriceVolumeProfile(obj);


            var maxKey = cummVolumeStats.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            var maxVolumeInDictionary = cummVolumeStats[maxKey];

            slopeStats = GetSlope(xUniformInts, prices.ToArray());

            Print(string.Format("Slope: {0}  Y-Intercept: {1}  R-Coeffecient: {2}", Math.Round(slopeStats[0], 4), Math.Round(slopeStats[1], 2), Math.Round(slopeStats[2], 4)));

            //Print(cummVolumeStats.Keys.Max());
            foreach (var data in cummVolumeStats)
            {
                string asteriskGuage = "";
                string outputSpacing = "";

                for (int i = 0; i < Math.Round(data.Value / maxVolumeInDictionary * 100, 0); i++)
                {
                    asteriskGuage = asteriskGuage + "|";
                }
                for (int i = 0; i < 110 - Math.Round(data.Value / maxVolumeInDictionary * 100,0); i++)
                {
                    outputSpacing = outputSpacing + " ";
                }

                Print(string.Format("{0}  {3}  {4} ({2}) {1}", data.Key.ToString("0.00"), data.Value, Math.Round(data.Value/maxVolumeInDictionary*100,0),asteriskGuage, outputSpacing));
            }

        }

        private SortedDictionary<double,double> GetCummalitivePriceVolumeProfile(Dictionary<string, PriceStack> obj)
        {

           SortedDictionary<double,double> stack = new SortedDictionary<double, double>();
            //Here I will calculate the cummalative price volume of the given stack. 
            foreach (var data in obj)
            {
                if (stack.ContainsKey(data.Value.Price) == true)
                {
                    stack[data.Value.Price] = stack[data.Value.Price] + data.Value.Volume;
                }
                else
                {
                    stack.Add(data.Value.Price,data.Value.Volume);
                }
            }

            return stack;

        }

        private void GetStdDeviationOfCloseAgainstRegressionLine()
        {
            // Conduct stants as to how often price was above or below the regression line and the degree in which this was pressent. 
        }
        private double[] GetSlope(int[] x, double[] y)
        {
            double[] returnStats = new double[3];

            //Print("Insde the GetSlope Method");

            double[] xyData = new double[Math.Max(x.Count(), y.Count())];
            double[] xxData = new double[Math.Max(x.Count(), y.Count())];
            double[] yyData = new double[Math.Max(x.Count(), y.Count())];

            double xSum = 0.0;
            double ySum = 0.0;
            double xySum = 0.0;
            double xxSum = 0.0;
            double yySum = 0.0;

            double m_slope = 0.0;
            double y_int = 0.0;
            double r_value = 0.0;

            for (int i = 0; i < xyData.Count(); i++)
            {
                xyData[i] = x[i] * y[i];
                xxData[i] = x[i] * x[i];
                yyData[i] = y[i] * y[i];

                xSum = xSum + x[i];
                ySum = ySum + y[i];
                xxSum = xxSum + xxData[i];
                xySum = xySum + xyData[i];
                yySum = yySum + yyData[i];

                //Print(string.Format("{0} * {1} = xy {2}  x^2 {3}  y^2  {4}", x[i], y[i], xyData[i], xxData[i], yyData[i], xSum, ySum, xySum, xxSum, yySum));

            }

            m_slope = (xyData.Count() * xySum - xSum * ySum) / (xyData.Count() * xxSum - (xSum * xSum));
            y_int = (ySum - m_slope * xSum) / xyData.Count();
            r_value = (xyData.Count() * xySum - xSum * ySum) /
                      Math.Sqrt((xyData.Count() * xxSum - xSum * xSum) * (xyData.Count() * yySum - ySum * ySum));

            returnStats[0] = m_slope;
            returnStats[1] = y_int;
            returnStats[2] = r_value;

            //About R Coeffecient 
            //http://blog.minitab.com/blog/adventures-in-statistics/regression-analysis-how-do-i-interpret-r-squared-and-assess-the-goodness-of-fit

            //Print(string.Format("slope_m {0}  int_y {1}  r {2}", m_slope, y_int, r_value));
            return returnStats;

            //Print(string.Format("sum(x) {0} sum(y) {1} sum(xy) {2} sum(x^2) {3} sum(y^2) {4} (sum(x))^2 {5}  (sum(y))^2 {6}", xSum, ySum, xySum, xxSum, yySum, (xSum * xSum), (ySum * ySum)));
            
        }
        
        #region Properties
        #endregion
    }

    public class PivotStat
{
        public enum OHLCSeries
        {
            Open,
            High,
            Low,
            Close
        }
        public OHLCSeries OHLC { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
    }
}

public class PriceStack
{
    public string Time { get; set; }
    public double Price { get; set; }
    public double Volume { get; set; }

}



#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private kOvernightExtremeTest[] cachekOvernightExtremeTest = null;

        private static kOvernightExtremeTest checkkOvernightExtremeTest = new kOvernightExtremeTest();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kOvernightExtremeTest kOvernightExtremeTest()
        {
            return kOvernightExtremeTest(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public kOvernightExtremeTest kOvernightExtremeTest(Data.IDataSeries input)
        {
            if (cachekOvernightExtremeTest != null)
                for (int idx = 0; idx < cachekOvernightExtremeTest.Length; idx++)
                    if (cachekOvernightExtremeTest[idx].EqualsInput(input))
                        return cachekOvernightExtremeTest[idx];

            lock (checkkOvernightExtremeTest)
            {
                if (cachekOvernightExtremeTest != null)
                    for (int idx = 0; idx < cachekOvernightExtremeTest.Length; idx++)
                        if (cachekOvernightExtremeTest[idx].EqualsInput(input))
                            return cachekOvernightExtremeTest[idx];

                kOvernightExtremeTest indicator = new kOvernightExtremeTest();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                Indicators.Add(indicator);
                indicator.SetUp();

                kOvernightExtremeTest[] tmp = new kOvernightExtremeTest[cachekOvernightExtremeTest == null ? 1 : cachekOvernightExtremeTest.Length + 1];
                if (cachekOvernightExtremeTest != null)
                    cachekOvernightExtremeTest.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cachekOvernightExtremeTest = tmp;
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
        public Indicator.kOvernightExtremeTest kOvernightExtremeTest()
        {
            return _indicator.kOvernightExtremeTest(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kOvernightExtremeTest kOvernightExtremeTest(Data.IDataSeries input)
        {
            return _indicator.kOvernightExtremeTest(input);
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
        public Indicator.kOvernightExtremeTest kOvernightExtremeTest()
        {
            return _indicator.kOvernightExtremeTest(Input);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.kOvernightExtremeTest kOvernightExtremeTest(Data.IDataSeries input)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.kOvernightExtremeTest(input);
        }
    }
}
#endregion
