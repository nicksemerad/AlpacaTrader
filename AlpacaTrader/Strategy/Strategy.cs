namespace Strategy;

using Component;

/// <summary>
///   This abstract class contains methods that any derived strategy will use and the abstract methods they will need
///   to implement in order to calculate signals based on a stock symbol's Bars that it's provided. 
/// </summary>
public abstract class Strategy
{
    /// <summary>
    ///   A single stock symbol's bars, which will be used to calculate this strategy's indicators.
    /// </summary>
    public List<Bar> Bars { get; set; }

    /// <summary>
    ///   Constructor that sets Bars to the passed list.
    /// </summary>
    /// <param name="bars">The list of bars to set this strat's bars to</param>
    protected Strategy(List<Bar> bars) => Bars = bars;

    /// <summary>
    ///   Adds a new bar to this strat's Bars, and handles any other process needed by a derived strategy.
    /// </summary>
    /// <param name="newBar">The newest bar to add to this strat's Bars</param>
    public abstract void Update(Bar newBar);

    /// <summary>
    ///   Gets a buy, hold, or sell signal based off of the strategy implementation. The buy, hold, and sell signals
    ///   are 1, 0, and -1 respectively. Calculates the signals based on the last Bar in Bars, and any strategy
    ///   indicators and conditions.
    /// </summary>
    /// <returns>The signal determined by this strategy, based on the latest bar in Bars.</returns>
    public abstract int GetSignal();
}