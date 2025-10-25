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

    /// <summary>
    ///   Get the latest bar for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest bar for</param>
    /// <returns>The api endpoint url for the latest bar(s)</returns>
    public static string LatestBars(List<string> symbols)
        => BuildDataEndpointUrl("bars", symbols);
    
    /// <summary>
    ///   Builds the endpoint url for a single page of the historical bars in the time from startTime to endTime for
    ///   the specified stock symbol. The timeframe parameter dictates the frequency of the bars wanted, formating
    ///   shown below. The numbers or number ranges inside the brackets are the limits for those time periods. For
    ///   example, timeframes in the minutes can only be from 1 to 59 minutes, as anything longer should use the hours
    ///   timeframes instead.
    ///   <list type="bullet">
    ///     <item>Minutes: [1-59]T</item>
    ///     <item>Hours: [1-23]H</item>
    ///     <item>Days: 1D</item>
    ///     <item>Weeks: 1W</item>
    ///     <item>Months: [1,2,3,4,6,12]M</item>
    ///   </list>
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical bars for</param>
    /// <param name="timeframe">The granularity of the historical bars i.e. one per hour, day, etc</param>
    /// <param name="startTime">DateTime the historical bars start at</param>
    /// <param name="endTime">DateTime the historical bars will end at</param>
    /// <param name="nextPageToken">The token needed to request the next page, if there is one</param>
    /// <returns>The api endpoint url for the next page of historical bars for the symbol</returns>
    public static string HistoricalBars(string symbol, string timeframe, DateTime startTime, DateTime endTime,
        string nextPageToken)
    {
        // set the endpoint base url and make the parameter list
        string baseUrl = $"{Data}/bars?";
        List<string> parameters =
        [
            $"symbols={symbol}",
            $"timeframe={timeframe}", 
            $"start={startTime.ToString(DateFormats.LongDateTimeFormat)}",
            $"end={endTime.ToString(DateFormats.LongDateTimeFormat)}",
            "limit=10000"
        ];
        
        // add the next page token if it is present
        if (!string.IsNullOrEmpty(nextPageToken))
            parameters.Add($"&page_token={nextPageToken}");
        
        // return the resulting url
        return ConcatUrlParameters(baseUrl, parameters);
    }

    /// <summary>
    ///   Get the latest bid and ask quotes for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest quotes for</param>
    /// <returns>The api endpoint url for the latest quotes</returns>
    public static string LatestQuotes(List<string> symbols)
        => $"{Data}/quotes/latest?symbols={string.Join(",", symbols)}";
    
    /// <summary>
    ///   Builds the endpoint url for a single page of the historical quotes in the time from startTime to endTime for
    ///   the specified stock symbol.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">DateTime the historical quotes start at</param>
    /// <param name="endTime">DateTime the historical quotes will end at</param>
    /// <param name="nextPageToken">The token needed to request the next page, if there is one</param>
    /// <returns>The api endpoint url for the next page of historical quotes for the symbol</returns>
    public static string HistoricalQuotes(string symbol, DateTime startTime, DateTime endTime, string nextPageToken)
    {
        // set the endpoint base url including the symbol and make the parameter list
        string baseUrl = $"{Data}/{symbol}/quotes?";
        List<string> parameters =
        [
            $"start={startTime.ToString(DateFormats.LongDateTimeFormat)}",
            $"end={endTime.ToString(DateFormats.LongDateTimeFormat)}",
            "limit=10000"
        ];
        
        // add the next page token if it is present
        if (!string.IsNullOrEmpty(nextPageToken))
            parameters.Add($"&page_token={nextPageToken}");
        
        // return the resulting url
        return ConcatUrlParameters(baseUrl, parameters);
    }
}