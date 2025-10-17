using Component;

namespace API;

/// <summary>
///   This class handles making and parsing requests to API endpoints.
/// </summary>
public class Client
{
    /// <summary>
    ///   Parses the LatestBar and LatestBars responses into Lists of Bars.
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public async Task<List<Bar>> GetLatestBars(List<string> symbols)
    {
        string end = Endpoint.LatestBars(["AAPL", "TSLA"]);
        Request request = new Request(end);
        string content = await request.GetAsync();
        Response res = new Response(content);
        return res.ParseBars();
    }

    /// <summary>
    ///   Uses pagination to get all the historical bars for the symbol that land within the start and end times. 
    /// </summary>
    /// <param name="symbols"></param>
    /// <param name="timeframe">[1-59]T, [1-23]H, 1D, 1W, [1,2,3,4,6,12]M</param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<List<Bar>> GetHistoricalBars(List<string> symbols, string timeframe, DateTime startTime,
        DateTime endTime)
    {
        string nextPageToken = null;
        List<Bar> bars = new List<Bar>();
        
        // keep requesting the endpoint and adding the parsed results until there is no next page
        do
        {
            string end = Endpoint.HistoricalBars(symbols, timeframe, startTime, endTime, nextPageToken);
            Request request = new Request(end);
            string content = await request.GetAsync();
            Response res = new Response(content);
        
            List<Bar> newBars = res.ParseHistoricalBars(out nextPageToken);
        
            if (newBars.Count > 0)
                bars.AddRange(newBars);
        
        } while (!string.IsNullOrEmpty(nextPageToken));
    
        return bars;
    }

    public static async Task Main(string[] args)
    {
        Client client = new Client();

        List<Bar> bars = await client.GetHistoricalBars(["AAPL"], "12H", DateTime.Today.AddDays(-365), DateTime.Today);
        Console.WriteLine(bars.Count);
    }
}