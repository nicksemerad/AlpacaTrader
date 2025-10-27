namespace Strategy;

using Component;

/// <summary>
///   This abstract class contains a number of methods that a strategy may use, specifically calculations for technical
///   indicators. There are also a number of abstract methods, which are methods that any Strategy class must
///   implement.
/// </summary>
public abstract class Strategy
{
    /// <summary>
    ///   When there is a new latest bar, use the past bars to determine the strategy's signal (i.e. buy, sell, wait)
    /// </summary>
    /// <param name="bars">The past bars</param>
    /// <param name="latestBar">The latest bar</param>
    /// <returns>The signal determined by this strategy</returns>
    public abstract int GetSignal(IList<Bar> bars, Bar latestBar);

    /// <summary>
    ///   Check that the two parameters that are needed to calculate a moving average are valid. The first parameter is
    ///   a list of bars, and the second is an integer that is the period of the moving average. The parameters are
    ///   considered valid if the following three conditions are met:
    ///   <list type="bullet">
    ///     <item>The period is greater than zero</item>
    ///     <item>The list of bars is not empty</item>
    ///     <item>The number of bars in the list is equal to or greater than the period</item>
    ///   </list>
    /// </summary>
    /// <param name="bars">The list holding the bars to calculate a moving average with</param>
    /// <param name="period">The period or number of bars to use in the calculation</param>
    /// <exception cref="ArgumentException">Throws if the parameters are invalid for any reason</exception>
    private static void CheckMovingAverageInputs(IList<Bar> bars, int period)
    {
        if (period == 0)
            throw new ArgumentException("Period must be greater than zero");
        if (!bars.Any())
            throw new ArgumentException("Bars list must have at least one bar");
        if (bars.Count < period)
            throw new ArgumentException($"Bars.Count ({bars.Count}) must be >= period ({period})");
    }

    /// <summary>
    ///   Get the Simple Moving Average (SMA) price of the "period" most recent bars. 
    /// </summary>
    /// <param name="bars">The list holding the bars to calculate the SMA with</param>
    /// <param name="period">The period or number of bars to use in the calculation</param>
    /// <returns>The SMA of the last period number of bar prices as a decimal</returns>
    public static decimal GetSma(IList<Bar> bars, int period)
    {
        CheckMovingAverageInputs(bars, period);
        return bars.TakeLast(period).Average(bar => bar.Close);
    }

    /// <summary>
    ///   Get the Exponential Moving Average (EMA) price of the "period" most recent bars. 
    /// </summary>
    /// <param name="bars">The list holding the bars to calculate the EMA with</param>
    /// <param name="period">The period or number of bars to use in the calculation</param>
    /// <returns>The EMA of the last period number of bar prices as a decimal</returns>
    public static decimal GetEma(IList<Bar> bars, int period)
    {
        // start with the SMA of the first period bars and use that as bars[period]'s EMA
        decimal ema = GetSma(bars.Take(period).ToList(), period);
        
        // calculate the multiplier that weighs the prices
        decimal multiplier = 2m / (period + 1);

        // apply the EMA calculation for every bar after the initial period
        foreach (Bar bar in bars.Skip(period))
            ema = NextEma(ema, bar.Close, multiplier);

        return ema;
        // return bars.Skip(period).Aggregate(ema, (current, bar) => NextEma(current, bar.Close, multiplier));
    }

    /// <summary>
    ///   Calculate the next EMA value using the current EMA, the new bar's closing price, and the multiplier factor
    ///   that is determined by the period.
    /// </summary>
    /// <param name="ema">The current EMA to use to calculate the next EMA</param>
    /// <param name="close">The new Bar's closing price to factor into the EMA</param>
    /// <param name="multiplier">The multiplier factor to use in the calculation</param>
    /// <returns>The new current EMA that was calculated using the new Bar's close price</returns>
    private static decimal NextEma(decimal ema, decimal close, decimal multiplier) => (close - ema) * multiplier + ema;

    public static void Main(string[] args)
    {
    }
}