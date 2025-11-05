namespace Strategy;

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
    ///   The bars representing a stock's historical prices. The first bar is the farthest in the past, and the last
    ///   bar is the most recent.
    /// </summary>
    private List<Bar> _bars;

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
    ///   Constructs a new RsiStrategy object with the provided bars and decision parameters.
    /// </summary>
    /// <param name="bars">The list of bars to start the RSI calculations with</param>
    /// <param name="period">The period to calculate the RSI with</param>
    /// <param name="buyLevel">The maximum RSI value where a buy signal should be made</param>
    /// <param name="sellLevel">The minimum RSI value where a sell signal should be made</param>
    public RsiStrategy(List<Bar> bars, int period = 14, decimal buyLevel = 30m, decimal sellLevel = 70m) : base(bars)
    {
        _bars = bars;
        _period = period;
        _buyLevel = buyLevel;
        _sellLevel = sellLevel;
        _rsiSeries = GetSeries();
    }

    /// <summary>
    ///   Returns the RSI series calculated with the _bars and _period.
    /// </summary>
    /// <returns></returns>
    public List<RsiResult> GetSeries() => Indicators.GetRsiSeries(_bars, _period);

    /// <summary>
    ///   Adds a new bar to the bars and updates the RSI series.
    /// </summary>
    /// <param name="newBar">The new bar to add to bars and recalculate the series for</param>
    private void Update(Bar newBar)
    {
        _bars.Add(newBar);
        _rsiSeries = GetSeries();
    }

    /// <summary>
    ///   Gets a buy, hold, or sell signal based off of the RSI.
    /// </summary>
    /// <returns></returns>
    public override int GetSignal()
    {
        // get the latest RSI
        var currentRsi = _rsiSeries.At(^1);

        // in warmup period, hold
        if (currentRsi == null)
            return 0;

        // RSI < 30 so its oversold, buy
        if (currentRsi < _buyLevel)
            return 1;

        // RSI > 70 so its overbought, sell
        if (currentRsi > _sellLevel)
            return -1;

        // RSI is somewhere in the middle, hold
        return 0;
    }
}