using System.Text;

namespace API;

/// <summary>
///   This static class provides a number of Alpaca API endpoint urls and handles their construction.
/// </summary>
public static class Endpoint
{
    private const string Trade = "https://paper-api.alpaca.markets";
    private const string Data = "https://data.alpaca.markets/v2/stocks";

    /// <summary>
    ///   The endpoint changes based on how many symbols are used.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="symbols"></param>
    /// <returns></returns>
    private static string BuildDataEndpointUrl(string name, List<string> symbols)
    {
        return symbols.Count > 1
            ? $"{Data}/{name}/latest?symbols={string.Join(",", symbols)}"
            : $"{Data}/{symbols}/{name}/latest";
    }

    public static string LatestBars(List<string> symbols)
        => BuildDataEndpointUrl("bars", symbols);

    public static string Snapshots(List<string> symbols)
        => BuildDataEndpointUrl("snapshots", symbols);

    public static string LatestTrades(List<string> symbols)
        => BuildDataEndpointUrl("trades", symbols);

    /// <summary>
    ///   Builds the endpoint url for the first page of the historical bars in the time from startTime to endTime.
    ///   timeframe is formatted as: [1-59]T, [1-23]H, 1D, 1W, [1,2,3,4,6,12]M
    ///   DateTime strings look like: "2022-01-03T09:00:00Z"
    /// </summary>
    /// <param name="symbols"></param>
    /// <param name="timeframe"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="nextPageToken"></param>
    /// <returns></returns>
    public static string HistoricalBars(List<string> symbols, string timeframe, DateTime startTime, DateTime endTime,
        string nextPageToken)
    {
        // add the params that are always present
        StringBuilder url = new StringBuilder($"{Data}/bars?");
        url.Append($"symbols={string.Join(",", symbols)}");
        url.Append($"&timeframe={timeframe}");
        url.Append($"&start={startTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
        url.Append($"&end={endTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
        
        // add the next page token if it is present
        if (!string.IsNullOrEmpty(nextPageToken))
            url.Append($"&page_token={nextPageToken}");
        
        // print and return the url
        Console.WriteLine(url.ToString());
        return url.ToString();
    }

    public static string Account() => $"{Trade}/v2/account";
    public static string Assets() => $"{Trade}/v2/assets";
}