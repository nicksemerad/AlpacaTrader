using Common;
using Component;
using Database;

namespace API;

/// <summary>
///   This class handles building API endpoint urls, sending requests, and parsing the response's data.
/// </summary>
public class Client
{
    /// <summary>
    ///   This private helper method handles the process that takes a specific endpoint url, requests it, gets the
    ///   response, and parses the response content, before returning a list of the parsed elements. This method takes
    ///   two parameters, one for the endpoint url being requested, and one for the function used to parse the
    ///   response json.
    /// </summary>
    /// <param name="url">The url to request and parse the response from</param>
    /// <param name="parser">A function that takes in a Response and returns a List of type T</param>
    /// <typeparam name="T">The data type of the objects that the endpoint returns a list of</typeparam>
    /// <returns>A list of the T elements that were parsed from the URL response json</returns>
    private static async Task<List<T>> UrlToParsedResponse<T>(string url, Func<Response, List<T>> parser)
    {
        Request request = new Request(url);
        string content = await request.GetAsync();
        Response res = new Response(content);
        return parser(res);
    }
    
    /// <summary>
    ///   Gets a list of the most recent Bars for the specified stock symbols The endpoint url is constructed,
    ///   requested, and parsed into the returned List of Bars.
    /// </summary>
    /// <param name="symbols">The stock ticker symbols to get the data for</param>
    /// <returns>A list of all the latest Bars returned from the endpoint</returns>
    public static async Task<List<Bar>> GetLatestBars(List<string> symbols)
    {
        return await UrlToParsedResponse<Bar>(Endpoints.LatestBars(symbols), r=>r.ParseBars());
    }
    
    /// <summary>
    ///   Gets a list of the most recent QuotePairs (ask and bid) for the specified stock symbols. The endpoint url
    ///   is constructed, requested, and parsed into the returned List of QuotePairs.
    /// </summary>
    /// <param name="symbols">The symbols to get the quotes for</param>
    /// <returns>A list of all the latest QuotePairs returned from the endpoint</returns>
    public static async Task<List<QuotePair>> GetLatestQuotes(List<string> symbols)
    {
        return await UrlToParsedResponse<QuotePair>(Endpoints.LatestQuotes(symbols), r=>r.ParseQuotes());
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
    public static async Task<List<Bar>> GetHistoricalBars(string symbol, string timeframe, DateTime startTime,
        DateTime endTime)
    {
        // start with no nextPageToken and an empty list of bars
        string nextPageToken = string.Empty;
        List<Bar> bars = new List<Bar>();
        
        // starting with no nextPageToken, request the first page holding the historical bars and the next
        // nextPageToken. Update nextPageToken and request again. This is repeated until there are no more pages and
        // all the historical bars have been retrieved.
        do 
        { 
            string endpointUrl = Endpoints.HistoricalBars(symbol, timeframe, startTime, endTime, nextPageToken);
            Request request = new Request(endpointUrl);
            string content = await request.GetAsync();
            Response res = new Response(content);
        
            // parse all the bars from the response and add them to the bars list. The nextPageToken ref
            // is passed to the parse method so it can be updated to the new next page token
            bars.AddRange(res.ParseHistoricalBars(ref nextPageToken));
            
        } while (!string.IsNullOrEmpty(nextPageToken));
    
        // return the list holding all the historical bars
        return bars;
    }

    /// <summary>
    ///   Uses the Historical Quotes API endpoint to get all the Quotes for the symbol that were made during the time
    ///   period defined by the parameters for start and end time.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">DateTime the historical quotes should start at</param>
    /// <param name="endTime">DateTime the historical quotes should end at</param>
    /// <returns>A list holding all the scraped historical quote pairs for the symbol</returns>
    public static async Task<List<QuotePair>> GetHistoricalQuotes(string symbol, DateTime startTime, DateTime endTime)
    {
        // start with no nextPageToken and an empty list of quote pairs
        string nextPageToken = string.Empty;
        List<QuotePair> quotePairs = new List<QuotePair>();
        
        // starting with no nextPageToken, request the first page holding the historical quotes and the next
        // nextPageToken. Update nextPageToken and request again. This is repeated until there are no more pages and
        // all the historical quotes have been retrieved.
        do 
        { 
            string endpointUrl = Endpoints.HistoricalQuotes(symbol, startTime, endTime, nextPageToken);
            Request request = new Request(endpointUrl);
            string content = await request.GetAsync();
            Response res = new Response(content);
            
            // parse all the quotes from the response and add them to the quotes list. The nextPageToken ref
            // is passed to the parse method so it can be updated to the new next page token
            quotePairs.AddRange(res.ParseHistoricalQuotes(ref nextPageToken));
            
        } while (!string.IsNullOrEmpty(nextPageToken));
    
        // return the list holding all the historical quote pairs
        return quotePairs;
    }

    
    public static async Task Main(string[] args)
    {
        List<Bar> bars = await GetLatestBars(["AAPL"]);
        Console.WriteLine($"\nTotal scraped quote pairs: {bars.Count}");
        foreach (Bar bar in bars.Take(1))
            Console.WriteLine(bar);
        
        List<QuotePair> quotes = await GetLatestQuotes(["AAPL"]);
        Console.WriteLine($"\nTotal scraped quote pairs: {bars.Count}");
        foreach (QuotePair quote in quotes.Take(1))
            Console.WriteLine(quote);
    }
}
