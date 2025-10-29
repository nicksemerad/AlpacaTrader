using Api;
using Common;
using Component;

namespace ApiTests;

/// <summary>
///   This class holds a number of tests for the Api project.
/// </summary>
[TestClass]
public sealed class ApiTests
{
    /// <summary>
    ///   Stock ticker symbol to use for the tests.
    /// </summary>
    private const string Symbol = "AAPL";
    
    /// <summary>
    ///   Four different DateTimes to use for the tests.
    /// </summary>
    private static List<DateTime> Times = [
        DateTimeUtils.ToDateTime("2025-10-27T13:30:00Z"), // market open
        DateTimeUtils.ToDateTime("2025-10-27T19:55:00Z"), // 5 minutes from close
        DateTimeUtils.ToDateTime("2025-10-27T20:00:00Z"), // market close
        DateTimeUtils.ToDateTime("2025-10-20T00:00:00Z") // a week earlier
    ];

    /// <summary>
    ///   Test that GetLatestBarsAsync successfully calls the API and retrieves the latest bar for AAPL.
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestBars()
    {
        List<Bar> bars = await Client.GetLatestBarsAsync([Symbol]);
        Assert.AreEqual(1, bars.Count);
        Assert.AreEqual(Symbol, bars[0].Symbol);
    }
    
    /// <summary>
    ///   Test that GetLatestQuotesAsync successfully calls the API and retrieves the latest quote(s) for AAPL.
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestQuotes()
    {
        List<QuotePair> quotes = await Client.GetLatestQuotesAsync([Symbol]);
        Assert.IsTrue(quotes.Count > 0);
        Assert.AreEqual(Symbol, quotes[0].Symbol);
    }
    
    /// <summary>
    ///   Test that GetHistoricalBarsAsync with the timeframe parameter at 30 minutes and the DateTime range from
    ///   market open until market close successfully calls the API and retrieves 13+/- 1 bars for AAPL, as the market
    ///   is open for 6:30, which has 13 periods of 30 minutes.
    /// </summary>
    [TestMethod]
    public async Task TestGetHistoricalBars()
    {
        DateTime start = Times[0], end = Times[2]; // market open until market close
        List<Bar> bars = await Client.GetHistoricalBarsAsync(Symbol, "30T", start, end); // 1 per 30 mins
        
        Assert.IsTrue(bars.Count > 0);
        Assert.AreEqual(13, bars.Count, 1); // market is open for 6 hours 30 mins, so 13 bars expected
        Assert.IsTrue(bars.All(b => b.Symbol == Symbol));
    }
    
    /// <summary>
    ///   Test that GetHistoricalQuotesAsync with the DateTime range from 5 minutes to close until close successfully
    ///   calls the API and retrieves the QuotePairs AAPL.
    /// </summary>
    [TestMethod]
    public async Task TestGetHistoricalQuotes()
    {
        DateTime start = Times[1], end = Times[2]; // 5 mins from market close until market close
        List<QuotePair> quotes = await Client.GetHistoricalQuotesAsync(Symbol, start, end);
        
        Assert.IsTrue(quotes.Count > 0);
        Assert.IsTrue(quotes.All(b => b.Symbol == Symbol));
    }
    
    /// <summary>
    ///   Test that GetTradingDaysAsync with the DateTime range from October 20th until October 27th successfully
    ///   calls the API and retrieves the dates of the 6 trading days that happened during that period. From Monday the
    ///   20th until Friday the 24th, which makes 5 days. Then the weekend where the market stayed closed, and ending
    ///   on Monday the 27th making 6 TradingDays.
    /// </summary>
    [TestMethod]
    public async Task TestGetTradingDays()
    {
        DateTime start = Times[3], end = Times[0]; // Oct 20 until Oct 27
        List<TradingDay> days = await Client.GetTradingDaysAsync(start, end);
        
        Assert.IsTrue(days.Count > 0);
        Assert.AreEqual(6, days.Count); // 20, 21, 22, 23, 24, weekend, 27
    }
}