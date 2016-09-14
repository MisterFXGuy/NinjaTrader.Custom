#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class kOvernightExtreme : Strategy
    {
        #region Variables
        TimeSpan RTHStartTime = new TimeSpan(09,30,00);
        TimeSpan RTHEndTime = new TimeSpan(16,00,00);
        TimeSpan IBEndTime = new TimeSpan(10,30,00);


        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            CalculateOnBarClose = true;
            Enabled = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>s
        protected override void OnBarUpdate()
        {
            if (CurrentBar == 0)
            {
                Print(Time[0]);

            }
        }

        #region Properties
        #endregion
    }
}

#region Wizard settings, neither change nor remove
/*@
<?xml version="1.0" encoding="utf-16"?>
<NinjaTrader>
  <Name>kOvernightExtreme</Name>
  <CalculateOnBarClose>True</CalculateOnBarClose>
  <Description>Enter the description of your strategy here</Description>
  <Parameters />
  <State>
    <CurrentState>
      <StrategyWizardState xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <Name>Flat</Name>
        <Sets />
        <StopTargets />
      </StrategyWizardState>
    </CurrentState>
  </State>
</NinjaTrader>
@*/
#endregion
