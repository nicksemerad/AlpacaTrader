namespace Strategy;

using Component;
using Skender.Stock.Indicators;
using Indicators;


/// <summary>
///   This class is an extremely basic trading strategy, and isn't implemented yet.
/// </summary>
public class BasicStrategy : StrategyBase
{
    /// <summary>
    ///   The bars representing a stock's historical prices.
    /// </summary>
    private List<Bar> _bars;
    private List<SmaResult> _smaResults;
    
    public BasicStrategy(List<Bar> bars) : base(bars)
    {
        _bars = bars;
        _smaResults = Indicators.GetSmaSeries(bars, 5);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void InitializeIndicators()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void UpdateIndicators()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bars"></param>
    /// <param name="currentBar"></param>
    /// <returns></returns>
    public override int GetSignal(IList<Bar> bars, Bar currentBar)
    {
        throw new NotImplementedException();
    }
}