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
    ///   This delegate represents a function using type T that takes in a Response object as its parameter and
    ///   returns a List of type T. This is intended to be used with Response class methods that parse a Response's
    ///   content into a list of whatever type the endpoint response's json represents.
    /// </summary>
    /// <typeparam name="T">The type of object that the response is parsed into a list of</typeparam>
    private delegate List<T> ResponseParser<T>(Response r);

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
    private static async Task<Response> UrlToResponse(string url)
    {
        Request request = new Request(url);
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
    private static async Task<List<T>> UrlToParsedPaginatedResponse<T>(string url, PaginatedResponseParser<T> parser)
    {
        // parse the first page using the parser and set the result as a new list, parser will update the token
        string token = string.Empty;
        List<T> listOfTs = parser(await UrlToResponse(url), ref token);

        // as long as the previous response had a next page token, add the token to the url and parse the response
        // (which updates token) and add the parsed T items to the list
        while (!string.IsNullOrEmpty(token))
        {
            Response r = await UrlToResponse(Endpoints.AddPaginationToken(url, token));
            listOfTs.AddRange(parser(r, ref token));
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
    public static async Task<List<Bar>> GetLatestBars(List<string> symbols)
    {
        return (await UrlToResponse(Endpoints.LatestBars(symbols))).ParseBars();
    }

    /// <summary>
    ///   Gets a list of the most recent QuotePairs (ask and bid) for the specified stock symbols. The endpoint url
    ///   is constructed, requested, and parsed into the returned List of QuotePairs.
    /// </summary>
    /// <param name="symbols">The symbols to get the quotes for</param>
    /// <returns>A list of all the latest QuotePairs returned from the endpoint</returns>
    public static async Task<List<QuotePair>> GetLatestQuotes(List<string> symbols)
    {
        return (await UrlToResponse(Endpoints.LatestQuotes(symbols))).ParseQuotes();
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
        return await UrlToParsedPaginatedResponse(
            Endpoints.HistoricalBars(symbol, timeframe, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalBars(ref token)
        );
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
        return await UrlToParsedPaginatedResponse(
            Endpoints.HistoricalQuotes(symbol, startTime, endTime),
            (Response r, ref string token) => r.ParseHistoricalQuotes(ref token)
        );
    }


    public static async Task Main(string[] args)
    {
        await HistoricalSample();
        // await LatestSample();
        // await DatabaseSample();
    }

    private static async Task HistoricalSample()
    {
        DateTime end = DateTime.Today.AddDays(-2), start = end.AddHours(-1);

        List<Bar> bars = await GetHistoricalBars("AAPL", "12H", start, end);
        Console.WriteLine($"\nTotal scraped bars: {bars.Count}");
        foreach (Bar bar in bars.Take(1))
            Console.WriteLine(bar);

        List<QuotePair> quotes = await GetHistoricalQuotes("AAPL", start, end);
        Console.WriteLine($"\nTotal scraped quote pairs: {quotes.Count}");
        foreach (QuotePair quote in quotes.Take(1))
            Console.WriteLine(quote);
    }

    private static async Task LatestSample()
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

    private static async Task DatabaseSample()
    {
        // Connect to the database first
        Console.WriteLine("Connecting to database");
        var dbConnection = new TradingDbConnection();
        if (!await dbConnection.IsDbConnectedAsync())
            return;

        Console.WriteLine("\nInitializing database");
        await dbConnection.InitializeDatabaseAsync();

        Console.WriteLine("\nScraping bars");
        DateTime start = DateTime.Today.AddDays(-5), end = DateTime.Today;
        List<Bar> bars = await GetHistoricalBars("AAPL", "12H", start, end);
        Console.WriteLine($"\nTotal scraped bars: {bars.Count}");

        Console.WriteLine("\nSaving bars to database");
        var barOps = new BarOperations();
        await barOps.InsertBarsAsync(bars);
        Console.WriteLine("Bars saved");

        Console.WriteLine("\nGetting bars from database");
        var dbBars = await barOps.GetBarsBySymbolAsync("AAPL", start, end);
        Console.WriteLine($"Total bars in database: {dbBars.Count}");

        foreach (Bar bar in dbBars.Take(3))
            Console.WriteLine(bar.ToString());
    }
}