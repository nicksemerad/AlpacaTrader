using Common;
using Database;
using Component;
using Microsoft.Extensions.Logging;

namespace Api;

/// <summary>
///   This class is the ETL Pipeline for Alpaca API data. It extracts the data from specific endpoints using Client
///   methods, which transforms the responses into custom c# objects. DbOperations methods are then used to load the
///   data objects into the trading_db SQL database, so they can be easily accessed without needing to repeat the API
///   calls.
/// </summary>
public static class DataPipeline
{
    /// <summary>
    ///   The ILogger used to log events in this class.
    /// </summary>
    private static readonly ILogger DataLog = Logger.Create(nameof(DataPipeline));

    /// <summary>
    ///   Private method to retrieve all the CalendarDays from the start of 2000 to the start of 2029 from the endpoint
    ///   API and write all the rows to the trading_calendar table.
    /// </summary>
    private static async Task WriteCalendarDaysToDbAsync(DateTime startDate, DateTime endDate)
    {
        // get the calendar days from the start date to the end date and insert them into the database
        var days = await Client.GetCalendarDaysAsync(startDate, endDate);
        await DbOperations.InsertCalendarDaysAsync(days);
        DataLog.LogDebug("Retrieved calendar days [{Start:yyyy} to {End:yyyy}]", startDate, endDate);
    }

    /// <summary>
    ///   This method starts by getting all the trading calendar days from the database that land within the time
    ///   range. These calendar days are then used along with a symbol to retrieve the symbol's bars for every
    ///   individual calendar day, from when the market opens until it closes, and write them to the database. This is
    ///   repeated for each of the symbols in the list parameter.
    /// </summary>
    /// <param name="symbols">The stock symbols to get the historical bars for</param>
    /// <param name="timeframe">The time between each bar</param>
    /// <param name="startDate">The date that the historical bars start</param>
    /// <param name="endDate">The date that the historical bars end</param>
    private static async Task WriteHistoricalBarsToDbAsync(List<string> symbols, string timeframe, DateTime startDate,
        DateTime endDate)
    {
        DataLog.LogInformation("Starting [{S}] {T} HistoricalBars from {Start:yyyy} to {End:yyyy}",
            string.Join(", ", symbols), timeframe, startDate, endDate);

        // get and write the historical bars for each symbol on each calendar day
        var calendar = await DbOperations.GetCalendarDaysAsync(startDate, endDate);
        foreach (var symbol in symbols)
            await WriteSymbolHistoricalBarsToDbAsync(symbol, timeframe, calendar);
    }

    /// <summary>
    ///   This method handles the actual API calls that are made to get the symbol's bars on each day in the calendar,
    ///   from market open until close. To prevent making an API call to get bars that have already been retrieved and
    ///   stored in the database, the AreBarsAlreadyInDb method is called at the start of the process for each day. If
    ///   there are already bars with the same parameters, the rest of the day's process is skipped, and the loop
    ///   continues onto the next calendar day. If the bars are yet to be retrieved, an API call is made for them and
    ///   they are stored in the database. After each API call, the thread waits for 300ms before continuing, in order
    ///   to not surpass the 200/minute rate limit that the Alpaca API has.
    /// </summary>
    /// <param name="symbol">The stock symbol to get the historical bars for</param>
    /// <param name="timeframe">The time between each bar</param>
    /// <param name="calendar">The calendar of all the trading days in the time period</param>
    private static async Task WriteSymbolHistoricalBarsToDbAsync(string symbol, string timeframe,
        List<CalendarDay> calendar)
    {
        DataLog.LogDebug("Retrieving {S} {T} HistoricalBars", symbol, timeframe);
        foreach (var day in calendar)
        {
            // if we already have the bars, skip to the next day
            if (await AreBarsAlreadyInDb(symbol, timeframe, day.OpenToUtc(), day.CloseToUtc())) continue;

            try
            {
                // try to get the bars from the API and insert them into the database
                var bars = await Client.GetHistoricalBarsAsync(symbol, timeframe, day.OpenToUtc(), day.CloseToUtc());
                var numRowsBefore = await DbOperations.GetBarsCountAsync();
                await DbOperations.InsertBarsAsync(bars);
                var numRowsAfter = await DbOperations.GetBarsCountAsync();
                DataLog.LogInformation("Retrieved {R:N0} bars from the API and {D:N0} were added to the Database",
                    bars.Count, numRowsAfter - numRowsBefore);

                // wait 300ms per API call to stay below the 200/min limit for Alpaca
                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                DataLog.LogError(ex, "Error: [{Symbol} {Date:yyyy}] {ExMessage}", symbol, day.Date, ex.Message);
            }
        }

        DataLog.LogInformation("All [{Symbol}] bars retrieved", symbol);
    }

    /// <summary>
    ///   Checks if there are already bars in the database that meet the same parameters. If there are then no API call
    ///   is needed so false is returned. If the bars with these specific parameters haven't been retrieved yet then an
    ///   API call is needed so true is returned.
    /// </summary>
    /// <param name="symbol">The stock symbol being checked</param>
    /// <param name="timeframe">The time between bars</param>
    /// <param name="start">The start of the time period to check</param>
    /// <param name="end">The end of the time period to check</param>
    /// <returns>A boolean, true if the bars are already in the database and false if not</returns>
    private static async Task<bool> AreBarsAlreadyInDb(string symbol, string timeframe, DateTime start, DateTime end)
    {
        // get all the bars in the database that exactly match the parameters
        var barsInDb = await DbOperations.GetBarsBySymbolTimeframeAsync(symbol, timeframe, start, end);

        // if the database doesn't have the bars return false
        if (barsInDb.Count == 0) return false;

        DataLog.LogDebug("Already have {Num:N0} {Symbol} bars from {Day:yyyy-MM-dd} in the database",
            barsInDb.Count, symbol, start.Date);
        return true;
    }

    /// <summary>
    ///   This method gets the calendar of trading days in the year range and writes them to the database. The
    ///   CalendarDays are then used to retrieve all the 1-minute bars for each day, from market open to close, for the
    ///   three starter stock symbols.
    /// </summary>
    private static async Task WriteDaysAndStarterStocks1MinBars(int startYear, int endYear)
    {
        DateTime start = new(startYear, 1, 1), end = new(endYear, 1, 1);

        // Get and write all the calendar days during the years
        // EXPECTED TIME TO RUN: ~3 seconds
        await WriteCalendarDaysToDbAsync(start, end);

        // the three starter stock symbols
        List<string> symbols = ["SPY", "QQQ", "AAPL"];

        // get any the symbols' 1-min bars for every trading day during the years
        // EXPECTED TIME TO RUN: ~20 minutes
        await WriteHistoricalBarsToDbAsync(symbols, "1T", start, end);

        // run the pipeline again to get any api calls that failed the first time
        // EXPECTED TIME TO RUN: ~1 minute
        await WriteHistoricalBarsToDbAsync(symbols, "1T", start, end);
    }

    public static async Task Main(string[] args)
    {
        // make and verify the connection before initializing db tables
        var dbc = new TradingDbConnection();
        if (await dbc.IsDbConnectedAsync())
            await dbc.InitializeDatabaseAsync();

        // get all the 1-minute bars for "SPY", "QQQ", "AAPL" for
        // every trading day from 1 Jan startYear to 1 Jan endYear
        await WriteDaysAndStarterStocks1MinBars(2020, 2025);
    }
}