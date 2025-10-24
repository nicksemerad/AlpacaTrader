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
    ///   Uses the Historical Bars API endpoint to get all the Bars for the symbol(s) according to the other
    ///   parameters. First is timeframe which describes the desired granularity of the historical bars. For example,
    ///   if we want one bar for every [1-59] minutes in the range timeframe is "[1-59]T". For hours timeframe can be
    ///   "[1-23]H". Following this pattern we can do 1 day, week, or common multiple of months with "1D", "1W", and
    ///   "[1,2,3,4,6,12]M" respectively. The last two parameters are DateTime objects that mark the start and end
    ///   dates of the requested historical bars.
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the historical bars for</param>
    /// <param name="timeframe">The granularity of the historical bars i.e. one per hour, day, etc</param>
    /// <param name="startTime">DateTime the historical bars start at</param>
    /// <param name="endTime">DateTime the historical bars will end at</param>
    /// <returns></returns>
    public async Task<List<Bar>> GetHistoricalBars(List<string> symbols, string timeframe, DateTime startTime,
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
            string endpointUrl = Endpoints.HistoricalBars(symbols, timeframe, startTime, endTime, nextPageToken);
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

    public static async Task Main(string[] args)
    {
        // Connect to the database first
        Console.WriteLine("Connecting to database");
        var dbConnection = new TradingDbConnection();
        if (!await dbConnection.IsDbConnectedAsync())
            return;

        Console.WriteLine("\nInitializing database");
        await dbConnection.InitializeDatabaseAsync();

        Console.WriteLine("\nScraping bars");
        Client client = new Client();
        DateTime start = DateTime.Today.AddDays(-5), end = DateTime.Today;
        List<Bar> bars = await client.GetHistoricalBars(["AAPL"], "12H", start, end);
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