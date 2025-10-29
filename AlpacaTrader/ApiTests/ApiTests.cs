using Api;
using Common;

namespace ApiTests;

/// <summary>
///   This class holds a number of tests for the Api project, focusing on the Client class methods as they use the
///   other two Api project classes (Request and Response) in conjunction with each other.
/// </summary>
[TestClass]
public sealed class ApiTests
{
    /// <summary>
    ///   Four different stock ticker symbols used during testing. They are all big tech stocks as each of they will
    ///   have plenty of trading activity at any given time.
    /// </summary>
    private static readonly List<string> Symbols = ["AAPL", "MSFT", "GOOG", "NVDA"];

    /// <summary>
    ///   Four different UTC DateTimes used during testing. For the shorter term tests the first three times are for
    ///   October 27th market open, the same day 5 minutes before market close, and market close. The last time is a
    ///   week before the first datetime in the list so the methods can be tested with a longer time range.
    /// </summary>
    private static readonly List<DateTime> Times =
    [
        DateTimeUtils.ToDateTime("2025-10-27T13:30:00Z"), // market open
        DateTimeUtils.ToDateTime("2025-10-27T19:55:00Z"), // 5 minutes from close
        DateTimeUtils.ToDateTime("2025-10-27T20:00:00Z"), // market close
        DateTimeUtils.ToDateTime("2025-10-21T00:00:00Z") // a week earlier (T, W, Th, F, S, Su, M)
    ];

    /// <summary>
    ///   Test that GetLatestBarsAsync with a single ticker successfully calls the API and retrieves the latest bar
    ///   for that ticker.
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestBars_OneSymbol_ReturnsOneSymbolBar()
    {
        var bars = await Client.GetLatestBarsAsync([Symbols[0]]);
        Assert.AreEqual(1, bars.Count);
        Assert.AreEqual(Symbols[0], bars[0].Symbol);
    }

    /// <summary>
    ///   Test that GetLatestBarsAsync with all four tickers successfully calls the API and retrieves the latest bar
    ///   for each one.
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestBars_ManySymbols_ReturnsEachSymbolsBar()
    {
        var bars = await Client.GetLatestBarsAsync(Symbols);
        Assert.AreEqual(4, bars.Count);

        foreach (var symbol in Symbols)
            Assert.IsTrue(bars.Any(b => b.Symbol == symbol));
    }

    /// <summary>
    ///   Test that GetLatestQuotesAsync with a single ticker successfully calls the API and retrieves the latest
    ///   quote(s) for that symbol.
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestQuotes_OneSymbol_ReturnsOneSymbolsQuotes()
    {
        var quotes = await Client.GetLatestQuotesAsync([Symbols[0]]);
        Assert.AreEqual(1, quotes.Count);
        Assert.AreEqual(Symbols[0], quotes[0].Symbol);
    }

    /// <summary>
    ///   Test that GetLatestQuotesAsync with all four tickers successfully calls the API and retrieves each of their
    ///   latest quote(s). Note: this API endpoint returns many different quotes for each symbol because the time
    /// </summary>
    [TestMethod]
    public async Task TestGetLatestQuotes_ManySymbol_ReturnsEachSymbolsQuotes()
    {
        var quotes = await Client.GetLatestQuotesAsync(Symbols);
        Assert.AreEqual(4, quotes.Count);

        foreach (var symbol in Symbols)
            Assert.IsTrue(quotes.Any(q => q.Symbol == symbol));
    }

    /// <summary>
    ///   Test that GetHistoricalBarsAsync with the timeframe parameter at 30 minutes and the DateTime range from
    ///   market open until market close successfully calls the API and retrieves 13 +/- 1 bars for AAPL, as the market
    ///   is open for 6:30, which has 13 periods of 30 minutes.
    /// </summary>
    [TestMethod]
    public async Task TestGetHistoricalBars_OneDayThirtyMinsBars_ReturnsExpectedBarsCount()
    {
        DateTime start = Times[0], end = Times[2]; // market open to market close (6.5 hours total)
        const string timeframe = "30T"; // bar timeframe of 30 mins (1 bar per 30 mins)
        const int expectedBarsPerDay = 13; // time range of 6.5 hours / 30 mins per bar = 13 bars expected

        var bars = await Client.GetHistoricalBarsAsync(Symbols[0], timeframe, start, end);

        Assert.IsTrue(bars.Count > 0);
        Assert.AreEqual(expectedBarsPerDay, bars.Count, 1); // delta of 1 (first/ last bars can be early/ late)
        Assert.IsTrue(bars.All(b => b.Symbol == Symbols[0]));
    }

    /// <summary>
    ///   Test that GetHistoricalQuotesAsync with the DateTime range from 5 minutes to close until close successfully
    ///   calls the API and retrieves many QuotePairs AAPL.
    /// </summary>
    [TestMethod]
    public async Task TestGetHistoricalQuotes_FiveMinSymbolQuotes_ReturnsManySymbolQuotes()
    {
        DateTime start = Times[1], end = Times[2]; // 5 mins from market close until market close
        var quotes = await Client.GetHistoricalQuotesAsync(Symbols[0], start, end);

        Assert.IsTrue(quotes.Count > 0);
        Assert.IsTrue(quotes.All(b => b.Symbol == Symbols[0]));
    }

    /// <summary>
    ///   Test that GetCalendarDaysAsync with the DateTime range from October 21st until October 27th successfully
    ///   calls the API and retrieves the dates of the 5 trading days that happened during that period. From Tuesday
    ///   the 21st until Friday the 24th makes the first 4 days. The market is closed over the weekend. Finally, on
    ///   Monday the 27th the market is open, bringing the total to 5 trading calendar days.
    /// </summary>
    [TestMethod]
    public async Task TestGetCalendarDays_SevenDayRange_ReturnsFiveCalendarDays()
    {
        DateTime start = Times[3], end = Times[0]; // Oct 21 until Oct 27
        var days = await Client.GetCalendarDaysAsync(start, end);
        Assert.IsTrue(days.Count > 0);
        Assert.AreEqual(5, days.Count); // (T, W, Th, F, S, Su, M)
    }
}