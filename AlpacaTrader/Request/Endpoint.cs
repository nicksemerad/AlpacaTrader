namespace Request;

/// <summary>
///   This static class provides a number of Alpaca API endpoint urls and handles their construction.
/// </summary>
public static class Endpoint
{
    private const string Trade = "https://paper-api.alpaca.markets";
    private const string Data = "https://data.alpaca.markets/v2/stocks";

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
    
    public static string Account() => $"{Trade}/v2/account";
    public static string Assets() => $"{Trade}/v2/assets";
}
