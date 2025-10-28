using Component;
using Skender.Stock.Indicators;

namespace Indicators;

/// <summary>
///   This class handles stock price indicators' calculations so they can be used in strategies, visualizations,
///   backtesting, etc.
/// </summary>
public static class Indicators
{
    public static void Main(string[] args)
    {
        Bar b = new Bar();
        // Quote q = new Quote(b.Timestamp, b.Open, b.High, b.Low, b.Close, b.Volume);
        List<Bar> quotes = [];
        IEnumerable<SmaResult> results = quotes.GetSma(20);
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
    }
}