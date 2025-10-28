using Component;

namespace Strategy;

/// <summary>
///   This class is an extremely basic trading strategy, and isn't implemented yet.
/// </summary>
public class BasicStrategy : StrategyBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="currentBar"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override int GetSignal(IList<Bar> bars, Bar currentBar)
    {
        throw new NotImplementedException();
    }
}