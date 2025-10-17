namespace Strategy;

using Component;

/// <summary>
///   This interface describes the necessary functionality that a trading strategy must have. This is just a
///   placeholder right now.
/// </summary>
public interface IStrategy
{
    /// <summary>
    ///  When there is a new latest bar, use the past bars to determine the strategy's signal (i.e. buy, sell, wait)
    /// </summary>
    /// <param name="bars">The past bars</param>
    /// <param name="latestBar">The latest bar</param>
    /// <returns>The signal determined by this strategy</returns>
    int GetSignal(List<Bar> bars, Bar latestBar);
}