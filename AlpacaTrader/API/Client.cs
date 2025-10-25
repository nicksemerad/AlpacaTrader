using Common;
using Component;
using Database;
using DbConnection = System.Data.Common.DbConnection;

namespace API;

/// <summary>
///   This class handles building API endpoint urls, sending requests, and parsing the response's data.
/// </summary>
public class Client
{
    /// <summary>
    ///   Gets a list of Bars that is retrieved from the LatestBars api endpoint for the provided symbols.
    /// </summary>
    /// <param name="symbols">The stock ticker symbols to get the data for</param>
    /// <returns>A list of all the bars returned from the endpoint</returns>
    public async Task<List<Bar>> GetLatestBars(List<string> symbols)
    {
        Request request = new Request(Endpoints.LatestBars(symbols));
        string content = await request.GetAsync();
        Response res = new Response(content);
        return res.ParseBars();
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
    public async Task<List<Bar>> GetHistoricalBars(string symbol, string timeframe, DateTime startTime,
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
    ///   Get the latest bid and ask quotes for the symbols.
    /// </summary>
    /// <param name="symbols">The symbols to get the quotes for</param>
    public async Task<List<QuotePair>> GetLatestQuotes(List<string> symbols)
    {
        Request request = new Request(Endpoints.LatestQuotes(symbols));
        string content = await request.GetAsync();
        Response res = new Response(content);
        return res.ParseQuotes();
    }

    /// <summary>
    ///   Uses the Historical Quotes API endpoint to get all the Quotes for the symbol that were made during the time
    ///   period defined by the parameters for start and end time.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">DateTime the historical quotes should start at</param>
    /// <param name="endTime">DateTime the historical quotes should end at</param>
    /// <returns>A list holding all the scraped historical quote pairs for the symbol</returns>
    public async Task<List<QuotePair>> GetHistoricalQuotes(string symbol, DateTime startTime,
        DateTime endTime)
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
        Client client = new Client();
        DateTime start = DateTime.Today.AddHours(-1), end = DateTime.Today;
        
        List<QuotePair> quotes = await client.GetHistoricalQuotes("AAPL", start, end);
        Console.WriteLine($"\nTotal scraped quote pairs: {quotes.Count}");
        
        foreach (QuotePair quote in quotes.Take(5))
            Console.WriteLine(quote);
        
    }
}
