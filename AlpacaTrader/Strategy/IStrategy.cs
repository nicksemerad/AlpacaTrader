namespace Strategy;

using Component;

/// <summary>
///   Explains what methods a strategy should have.
/// </summary>
public interface IStrategy
{
    /// <summary>
    ///   Determine if a buy, sell, or nothing should occur.
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="currentBar"></param>
    /// <returns></returns>
    int GetSignal(List<Bar> bars, Bar currentBar);
}