namespace Strategy;

using Component;

/// <summary>
///   This abstract class contains methods that any derived strategy needs to implement, and will use to calculate
///   signals based on the bars it is provided. 
/// </summary>
public abstract class Strategy
{
    /// <summary>
    ///   The bars representing a stock's historical prices.
    /// </summary>
    private List<Bar> _bars;
    
    /// <summary>
    ///   Gets the current list of bars.
    /// </summary>
    protected List<Bar> GetBars() => _bars;

    /// <summary>
    ///   Base constructor for any strategies. Sets _bars to the passed list of bars.
    /// </summary>
    /// <param name="bars"></param>
    protected Strategy(List<Bar> bars) => _bars = bars;

    /// <summary>
    ///   Gets a buy, hold, or sell signal based off of the strategy implementation.
    /// </summary>
    /// <returns>The signal determined by this strategy</returns>
    public abstract int GetSignal();
}