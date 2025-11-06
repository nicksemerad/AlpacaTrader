namespace Strategy;

using Common;
using Component;
using Skender.Stock.Indicators;
using Indicators;

/// <summary>
///   This class is the first trading strategy I am trying out, it uses only RSI to determine signals. I haven't made
///   any trading strategies before, so this is a learning experience for that along with all the coding stuff that is
///   new to me as well. Will definitely make more complex strategies in the future, but this one is just the one I'll
///   use to make sure things are working as expected with Backtest and whatever else.
/// </summary>
public class RsiStrategy : Strategy
{
    /// <summary>
    ///   The series of RSI values for the strategy's bars. Calculated with a period of 14, or whatever was passed into
    ///   the constructor when the strategy was first instantiated. 
    /// </summary>
    private List<RsiResult> _rsiSeries;

    /// <summary>
    ///   The period to calculate the RSI with, default is 14.
    /// </summary>
    private readonly int _period;

    /// <summary>
    ///   The RSI value at which a signal to buy should be made. Defaults to 30 (oversold).
    /// </summary>
    private readonly decimal _buyLevel;

    /// <summary>
    ///   The RSI value at which a signal to sell should be made. Defaults to 70 (overbought).
    /// </summary>
    private readonly decimal _sellLevel;

    /// <summary>
    ///   Constructs a new RsiStrategy object with the provided bars and decision parameters. The two parameters
    ///   buyLevel and sellLevel are the RSI value levels (0-100) where a buy or sell signal should be triggered. The
    ///   buyLevel defaults to 30, meaning when the RSI drops to 30 or below a buy is signaled. The sellLevel defaults
    ///   to 70, so when the RSI is at or above 70 a sell is signaled.
    /// </summary>
    /// <param name="bars">The list of bars to start the strategy with, and use to calc RSI values</param>
    /// <param name="period">The period to calculate the RSI with</param>
    /// <param name="buyLevel">The maximum RSI value where a buy signal should be made</param>
    /// <param name="sellLevel">The minimum RSI value where a sell signal should be made</param>
    public RsiStrategy(List<Bar> bars, int period = 14, decimal buyLevel = 30m, decimal sellLevel = 70m) : base(bars)
    {
        Bars = bars;
        _period = period;
        _buyLevel = buyLevel;
        _sellLevel = sellLevel;
        _rsiSeries = GetSeries();
    }

    /// <summary>
    ///   Returns the RSI series calculated with the _bars and _period.
    /// </summary>
    /// <returns></returns>
    private List<RsiResult> GetSeries() => Indicators.GetRsiSeries(Bars, _period);

    /// <summary>
    ///   Adds a new bar to the bars and updates the RSI series.
    /// </summary>
    /// <param name="newBar">The new bar to add to bars and recalculate the series for</param>
    public override void Update(Bar newBar)
    {
        Bars.Add(newBar);
        _rsiSeries = GetSeries();
    }

    /// <summary>
    ///   Gets a buy, hold, or sell signal based off of the RSI.
    /// </summary>
    /// <returns></returns>
    public override TradeSignal GetSignal()
    {
        // get the latest RSI
        var currentRsi = _rsiSeries.At(^1);

        // in warmup period, hold
        if (currentRsi == null)
            return TradeSignal.Hold;

        // RSI < 30 so its oversold, buy
        if (currentRsi < _buyLevel)
            return TradeSignal.Buy;

        // RSI > 70 so its overbought, sell
        if (currentRsi > _sellLevel)
            return TradeSignal.Sell;

        // RSI is somewhere in the middle, hold
        return TradeSignal.Hold;
    }
}