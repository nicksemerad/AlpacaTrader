using Common;
using Component;

namespace API;

/// <summary>
///   This class handles building API endpoint urls, sending requests, and parsing the response's data.
/// </summary>
public class Client
{
    /// <summary>
    ///   This delegate is similar to a ResponseParser but has an additional parameter, which is a string holding the
    ///   next page token, passed by reference. This lets the parser update the reference for the next page token that
    ///   is passed into it, enabling pagination while parsing response content.
    /// </summary>
    /// <typeparam name="T">The type of object that the response is parsed into a list of</typeparam>
    private delegate List<T> PaginatedResponseParser<T>(Response r, ref string token);

    /// <summary>
    ///   This method takes in an endpoint url and returns a new response object with the json content returned from
    ///   the endpoint request.
    /// </summary>
    /// <param name="url">The url to request and return in a Response</param>
    /// <returns>A response object with the json content returned by the request</returns>
    private static async Task<Response> GetResponseAsync(string url)
    {
        var request = new Request(url);
        string content = await request.GetAsync();
        return new Response(content);
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
        // parse the first page using the parser and set the result as a new list, parser will update the token
        var token = string.Empty;
        List<T> listOfTs = parser(await GetResponseAsync(url), ref token);

        // as long as the previous response had a next page token, add the token to the base url and parse the
        // response (which updates token) and add the parsed Ts to the list
        while (!string.IsNullOrEmpty(token))
        {
            var response = await GetResponseAsync(Endpoints.AddPaginationToken(url, token));
            listOfTs.AddRange(parser(response, ref token));
        }

        // all the paginated data pages have been collected, return the final list
        return listOfTs;
    }

    /// <summary>
    ///   Gets a list of the most recent Bars for the specified stock symbols The endpoint url is constructed,
    ///   requested, and parsed into the returned List of Bars.
    /// </summary>
    /// <param name="symbols">The stock ticker symbols to get the data for</param>
    /// <returns>A list of all the latest Bars returned from the endpoint</returns>
    public static async Task<List<Bar>> GetLatestBarsAsync(List<string> symbols)
    {
        var response = await GetResponseAsync(Endpoints.LatestBars(symbols));
        return response.ParseBars();
    }

    /// <summary>
    ///   Gets a list of the most recent QuotePairs (ask and bid) for the specified stock symbols. The endpoint url
    ///   is constructed, requested, and parsed into the returned List of QuotePairs.
    /// </summary>
    /// <param name="symbols">The symbols to get the quotes for</param>
    /// <returns>A list of all the latest QuotePairs returned from the endpoint</returns>
    public static async Task<List<QuotePair>> GetLatestQuotesAsync(List<string> symbols)
    {
        var response = await GetResponseAsync(Endpoints.LatestQuotes(symbols));
        return response.ParseQuotes();
    }

    /// <summary>
    ///   Uses the Historical Bars API endpoint to get all the Bars for the symbol according to the other
    ///   parameters. First is timeframe which describes the desired granularity of the historical bars. For example,
    ///   if we want one bar for every [1-59] minutes in the range timeframe is "[1-59]T". For hours timeframe can be
    ///   "[1-23]H". Following this pattern we can do 1 day, week, or common multiple of months with "1D", "1W", and
    ///   "[1,2,3,4,6,12]M" respectively. The last two parameters are DateTime objects that mark the start and end
    ///   dates of the requested historical bars.
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
        var historicalBars = await GetAllPaginatedItemsAsync(
            Endpoints.HistoricalBars(symbol, timeframe, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalBars(ref token)
        );

        // set the symbol and timeframe properties for every bar in the list
        foreach (var bar in historicalBars)
        {
            bar.Symbol = symbol;
            bar.Timeframe = timeframe;
        }

        return historicalBars;
    }

    /// <summary>
    ///   Uses the Historical Quotes API endpoint to get all the Quotes for the symbol that were made during the time
    ///   period defined by the parameters for start and end time.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">DateTime the historical quotes should start at</param>
    /// <param name="endTime">DateTime the historical quotes should end at</param>
    /// <returns>A list holding all the scraped historical quote pairs for the symbol</returns>
    public static async Task<List<QuotePair>> GetHistoricalQuotesAsync(string symbol, DateTime startTime,
        DateTime endTime)
    {
        return await GetAllPaginatedItemsAsync(
            Endpoints.HistoricalQuotes(symbol, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalQuotes(ref token)
        );
    }
}