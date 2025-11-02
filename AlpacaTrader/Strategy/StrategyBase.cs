namespace Strategy;

using Component;

/// <summary>
///   This abstract class contains methods that any derived strategy needs to implement, and will use to calculate
///   signals based on the bars it is provided. 
/// </summary>
public abstract class StrategyBase
{
    /// <summary>
    ///   The bars representing a stock's historical prices.
    /// </summary>
    private List<Bar> _bars;

    protected StrategyBase(List<Bar> bars) => _bars = bars;

    /// <summary>
    ///   Initializes the list of historical bars as well as the technical indicators that are used to calculate
    ///   signals.
    /// </summary>
    /// <param name="historicalBars"></param>
    public void Initialize(List<Bar> historicalBars)
    {
        _bars = historicalBars;
        InitializeIndicators();
    }

    /// <summary>
    ///   Adds a new bar to the list of bars, which updates the current price history.
    /// </summary>
    /// <param name="newBar"></param>
    public void Update(Bar newBar)
    {
        _bars.Add(newBar);
        UpdateIndicators();
    }

    /// <summary>
    ///   Initializes the technical indicators that a strategy uses to calculate signals.
    /// </summary>
    protected abstract void InitializeIndicators();

    /// <summary>
    ///   Updates the technical indicators that a strategy uses to calculate signals.
    /// </summary>
    protected abstract void UpdateIndicators();

    /// <summary>
    ///   When there is a new latest bar, use the past bars to determine the strategy's signal (i.e. buy, sell, hold)
    /// </summary>
    /// <param name="bars">The past bars</param>
    /// <param name="latestBar">The latest bar</param>
    /// <returns>The signal determined by this strategy</returns>
    public abstract int GetSignal(IList<Bar> bars, Bar latestBar);


    public static void Main(string[] args)
    {
    }
}