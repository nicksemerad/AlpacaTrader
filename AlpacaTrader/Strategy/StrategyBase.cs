namespace Strategy;

using Component;

/// <summary>
///   This abstract class contains a number of methods that a strategy may use, specifically calculations for technical
///   indicators. There are also a number of abstract methods, which are methods that any Strategy class must
///   implement.
/// </summary>
public abstract class StrategyBase
{
    private IList<Bar> _bars;


    /// <summary>
    ///   When there is a new latest bar, use the past bars to determine the strategy's signal (i.e. buy, sell, wait)
    /// </summary>
    /// <param name="bars">The past bars</param>
    /// <param name="latestBar">The latest bar</param>
    /// <returns>The signal determined by this strategy</returns>
    public abstract int GetSignal(IList<Bar> bars, Bar latestBar);


    public static void Main(string[] args)
    {
    }
}