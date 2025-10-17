namespace Common;


/// <summary>
///   This class holds common Alpaca API endpoint urls and handles their construction.
/// </summary>
public static class Endpoints
{
    /// <summary>
    ///   The base url for all api endpoints needed when making paper trades.
    /// </summary>
    private const string Trade = "https://paper-api.alpaca.markets";

    /// <summary>
    ///   The base url for all api endpoints needed to get stock data.
    /// </summary>
    private const string Data = "https://data.alpaca.markets/v2/stocks";
    
    /// <summary>
    ///   Build the correct endpoint for getting the latest endpoint name using the passed symbols. The endpoint format
    ///   changes depending on the number of symbols
    /// </summary>
    /// <param name="name">The name of the desired endpoint data i.e. bars, trades, etc</param>
    /// <param name="symbols">The stock ticker symbol(s) to get the data for</param>
    /// <returns>The full api endpoint url for the parameters</returns>
    private static string BuildDataEndpointUrl(string name, List<string> symbols)
    {
        return symbols.Count > 1
            ? $"{Data}/{name}/latest?symbols={string.Join(",", symbols)}"
            : $"{Data}/{symbols}/{name}/latest";
    }

    /// <summary>
    ///   Get the latest bar for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest bar for</param>
    /// <returns>The api endpoint url for the latest bar(s)</returns>
    public static string LatestBars(List<string> symbols)
        => BuildDataEndpointUrl("bars", symbols);

    /// <summary>
    ///   Get the latest snapshot for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest snapshot for</param>
    /// <returns>The api endpoint url for the latest snapshot(s)</returns>
    public static string Snapshots(List<string> symbols)
        => BuildDataEndpointUrl("snapshots", symbols);

    /// <summary>
    ///   Get the latest trade for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest trade for</param>
    /// <returns>The api endpoint url for the latest trade(s)</returns>
    public static string LatestTrades(List<string> symbols)
        => BuildDataEndpointUrl("trades", symbols);

    /// <summary>
    ///   Builds the endpoint url for the first page of the historical bars in the time from startTime to endTime.
    ///   timeframe is formatted as: [1-59]T, [1-23]H, 1D, 1W, [1,2,3,4,6,12]M
    ///   DateTime strings look like: "2022-01-03T09:00:00Z"
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the historical bars for</param>
    /// <param name="timeframe">The granularity of the historical bars i.e. one per hour, day, etc</param>
    /// <param name="startTime">DateTime the historical bars start at</param>
    /// <param name="endTime">DateTime the historical bars will end at</param>
    /// <param name="nextPageToken">The token needed to request the next page, if there is one</param>
    /// <returns>The api endpoint url for the next page of historical bars for the symbols</returns>
    public static string HistoricalBars(List<string> symbols, string timeframe, DateTime startTime, DateTime endTime,
        string nextPageToken)
    {
        // set the endpoint base url including the symbols, and the url parameter values
        string baseUrl = $"{Data}/bars?symbols={string.Join(",", symbols)}";
        List<string> parameters =
        [
            $"&timeframe={timeframe}", 
            $"&start={startTime.ToString(DateFormats.LongDateTimeFormat)}",
            $"&end={endTime.ToString(DateFormats.LongDateTimeFormat)}"
        ];
        
        // add the next page token if it is present
        if (!string.IsNullOrEmpty(nextPageToken))
            parameters.Add($"&page_token={nextPageToken}");
        
        // return the resulting url
        return ConcatUrlParameters(baseUrl, parameters);
    }

    /// <summary>
    ///   Concatenates all the parameter strings onto the base url with an ampersand ('&') in between each parameter.
    /// </summary>
    /// <param name="baseUrl">The first segment of the endpoint url including the symbols parameters</param>
    /// <param name="parameterStrings">All the other string parameters to add to the endpoint url </param>
    /// <returns>The endpoint url with all the parameters concatenated together</returns>
    private static string ConcatUrlParameters(string baseUrl, List<string> parameterStrings)
    {
        return $"{baseUrl}&{string.Join("&", parameterStrings)}";
    }

    /// <summary>
    ///   Get the api endpoint url to get the account's trading information.
    /// </summary>
    /// <returns>The api endpoint url for the account's trading information</returns>
    public static string Account() => $"{Trade}/v2/account";

    /// <summary>
    ///   Get the api endpoint url to get the account's assets.
    /// </summary>
    /// <returns>The api endpoint url for the account assets</returns>
    public static string Assets() => $"{Trade}/v2/assets";
}