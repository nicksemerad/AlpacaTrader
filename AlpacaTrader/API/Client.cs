using Common;
using Component;
using Microsoft.Extensions.Logging;

namespace Api;

/// <summary>
///   This class orchestrates the processes that retrieve and store Alpaca API data. This is done by using many
///   project classes in conjunction with each-other. The static Endpoint methods take the necessary parameters and
///   uses them to build the API URLs. The URLs are then passed to the Request class which makes the http requests and
///   retrieves the API's JSON text response. This JSON text is passed to the Response class, which handles parsing it
///   into c# objects like Bars, Quotes, Orders, CalendarDays, etc. depending on the endpoint and parameters. Once the
///   data has been retrieved, parsed, and aggregated, it is returned as a List.
/// </summary>
public static class Client
{
    /// <summary>
    ///   The ILogger used to log events in this class.
    /// </summary>
    private static readonly ILogger ClientLog = Logger.Create(nameof(Client));

    /// <summary>
    ///   This delegate is similar to a ResponseParser but has an additional parameter, which is a string holding the
    ///   next page token, passed by reference. This lets the parser update the reference for the next page token that
    ///   is passed into it, enabling pagination while parsing response content.
    /// </summary>
    /// <typeparam name="T">The type of object that the response is parsed into a list of</typeparam>
    private delegate List<T> PaginatedResponseParser<T>(Response r, ref string token);

    /// <summary>
    ///   This method takes in an endpoint url and returns a new response object storing the json content that was
    ///   returned from requesting the endpoint.
    /// </summary>
    /// <param name="url">The url to request and return in a Response</param>
    /// <returns>A response object with the json content returned by the request</returns>
    private static async Task<Response> GetResponseAsync(string url)
    {
        var request = new Request(url);
        var contentString = await request.GetAsync();
        return new Response(contentString);
    }

    /// <summary>
    ///   Using the url and a specified function to parse the results with, request the first page of the endpoint and
    ///   use the next page token in the response to request the next page, and repeat until there are no next pages.
    /// </summary>
    /// <param name="url">The url of the first page of the endpoint data</param>
    /// <param name="parser">The function to parse the endpoint responses with</param>
    /// <typeparam name="T">The type of data in the list that is parsed from the endpoint</typeparam>
    /// <returns>A list with all the collected elements from the endpoint</returns>
    private static async Task<List<T>> GetAllPaginatedItemsAsync<T>(string url, PaginatedResponseParser<T> parser)
    {
        // parse the first page using the parser and set the result as the items list. parser also updates the token
        var token = string.Empty;
        var pageItems = parser(await GetResponseAsync(url), ref token);

        // if the previous response has a next page token, add it to the base url, request it, and parse the response
        // (which updates token). add the parsed items to the pageItems list and repeat until token is null or empty
        while (!string.IsNullOrEmpty(token))
        {
            var response = await GetResponseAsync(Endpoints.AddPaginationToken(url, token));
            pageItems.AddRange(parser(response, ref token));
        }

        // all the paginated data pages have been collected, return the final list
        return pageItems;
    }

    /// <summary>
    ///   Gets a list with the most recent Bar for each of the specified stock symbols.
    /// </summary>
    /// <param name="symbols">The stock ticker symbols to get the latest bars for</param>
    /// <returns>A list of all the latest Bars returned from the endpoint</returns>
    public static async Task<List<Bar>> GetLatestBarsAsync(List<string> symbols)
    {
        // get the latest bars page for the symbols and parse them all into a list
        var response = await GetResponseAsync(Endpoints.LatestBars(symbols));
        var bars = response.ParseBars();

        // log the results before returning the list
        ClientLog.LogDebug("[{Symbols}]: LatestBars retrieved {Count} latest quotes",
            string.Join(", ", symbols), $"{bars.Count:N0}");
        return bars;
    }

    /// <summary>
    ///   Gets a list of the most recent QuotePairs for the specified stock symbols, which consists of an Ask Quote,
    ///   a Bid Quote, or both.
    /// </summary>
    /// <param name="symbols">The symbols to get the quotes for</param>
    /// <returns>A list of all the latest QuotePairs returned from the endpoint</returns>
    public static async Task<List<QuotePair>> GetLatestQuotesAsync(List<string> symbols)
    {
        // get the latest quotes page for the symbols and parse them all into a list
        var response = await GetResponseAsync(Endpoints.LatestQuotes(symbols));
        var quotes = response.ParseQuotes();

        // log the results before returning the list
        ClientLog.LogDebug("[{Symbols}]: LatestQuotes retrieved {Count} latest quotes",
            string.Join(", ", symbols), $"{quotes.Count:N0}");
        return quotes;
    }

    /// <summary>
    ///   Uses the Historical Bars API endpoint to get all the Bars for the symbol according to the other
    ///   parameters. The first additional parameter is timeframe, which describes the desired time period between the
    ///   historical bars. As with any historical data and endpoints, the final two parameters are the start and end
    ///   date time for the range of desired data. The timeframe string can be any of the following:
    ///   <list type="bullet">
    ///     <item>Minutes: [1-59]T</item>
    ///     <item>Hours: [1-23]H</item>
    ///     <item>Days: 1D</item>
    ///     <item>Weeks: 1W</item>
    ///     <item>Months: [1, 2, 3, 4, 6, 12]M</item>
    ///   </list>
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical bars for</param>
    /// <param name="timeframe">The granularity of the historical bars i.e. one per hour, day, etc</param>
    /// <param name="startTime">DateTime the historical bars start at</param>
    /// <param name="endTime">DateTime the historical bars will end at</param>
    /// <returns>A list holding all the scraped historical bars for the symbol</returns>
    public static async Task<List<Bar>> GetHistoricalBarsAsync(string symbol, string timeframe, DateTime startTime,
        DateTime endTime)
    {
        // get all the historical bars for the symbol, timeframe, and time range
        var bars = await GetAllPaginatedItemsAsync(
            Endpoints.HistoricalBars(symbol, timeframe, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalBars(ref token)
        );

        // log the results before returning the list (and adding the symbol/ timeframe in this case)
        ClientLog.LogDebug("[{Symbol}(1 bar/ {Timeframe})]: HistoricalBars {Start} to {End} retrieved {Count} bars",
            symbol, timeframe, $"'{startTime:yy-MM-dd}", $"'{endTime:yy-MM-dd}", $"{bars.Count:N0}");

        // set the symbol and timeframe properties for every bar in the list before returning
        return bars.Select(bar =>
        {
            bar.Symbol = symbol;
            bar.Timeframe = timeframe;
            return bar;
        }).ToList();
    }

    /// <summary>
    ///   Uses the Historical Quotes API endpoint to get all the ask and bid Quotes made for the symbol during the
    ///   time range defined by the start and end time parameters.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">The DateTime that the historical quotes start at</param>
    /// <param name="endTime">The DateTime that the historical quotes end at</param>
    /// <returns>A list holding all the parsed historical quote pairs for the symbol</returns>
    public static async Task<List<QuotePair>> GetHistoricalQuotesAsync(string symbol, DateTime startTime,
        DateTime endTime)
    {
        // get all the historical quotes for the symbol in the time range
        var quotes = await GetAllPaginatedItemsAsync(
            Endpoints.HistoricalQuotes(symbol, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalQuotes(ref token)
        );

        // log the results before returning the list
        ClientLog.LogDebug("[{Symbol}]: HistoricalQuotes [{Start} to {End}] retrieved {Count} quotes",
            symbol, $"'{startTime:yy-MM-dd}", $"'{endTime:yy-MM-dd}", $"{quotes.Count:N0}");
        return quotes;
    }

    /// <summary>
    ///   Gets a list of CalendarDay objects that are parsed from the calendar endpoint. The calendar endpoint holds
    ///   all the active calendar days and the times for open/close, pre-market open/ post-market close, etc. for the
    ///   specified time range. This data will be used primarily during backtesting in order to simulate trading
    ///   during actual trading hours. 
    /// </summary>
    /// <param name="startTime">DateTime the list of CalendarDays starts at</param>
    /// <param name="endTime">DateTime the list of CalendarDays ends at</param>
    /// <returns>A list of all the trading calendar days' info within the date range</returns>
    public static async Task<List<CalendarDay>> GetCalendarDaysAsync(DateTime startTime, DateTime endTime)
    {
        // get and parse the CalendarDays that fall within the time range
        var response = await GetResponseAsync(Endpoints.Calendar(startTime, endTime));
        var days = response.ParseCalendarDays();

        // log the results before returning the list
        ClientLog.LogDebug("CalendarDays [{Start} to {End}] retrieved {Count} trading day dates",
            $"'{startTime:yy-MM-dd}", $"'{endTime:yy-MM-dd}", $"{days.Count:N0}");
        return days;
    }
}