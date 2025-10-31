using Component;
using Database;
using Skender.Stock.Indicators;

namespace Indicators;

/// <summary>
///   This class handles stock price indicators' calculations so they can be used in strategies, visualizations,
///   backtesting, etc.
/// </summary>
public static class Indicators
{
    public static async Task Main(string[] args)
    {
        List<Bar> quotes = await DbOperations.GetBarsBySymbolTimeframeAsync(
            "SPY", "1T", new DateTime(2023, 1, 1), new DateTime(2023, 6, 1));

        var results = quotes.GetMacd().ToList();

        double? min = 0;
        double? max = 0;
        
        foreach (var value in results.Select(t => t.Macd))
        {
            if (value < min) min = value;
            if (value > max) max = value;
        }
        
        Console.WriteLine($"COUNT: {results.Count:N0}");
        Console.WriteLine($"MAX: {max:F6}");
        Console.WriteLine($"MIN: {min:F6}");
        Console.WriteLine($"RANGE: {max - min:F6}");
    }
}