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
    ///   Build the endpoint for getting the latest data from the endpoint name using the passed symbols.
    /// </summary>
    /// <param name="name">The name of the desired endpoint data i.e. bars, quotes, etc</param>
    /// <param name="symbols">The stock ticker symbol(s) to get the data for</param>
    /// <returns>The full api endpoint url for the parameters</returns>
    private static string LatestUrl(string name, List<string> symbols)
    {
        return $"{Data}/{name}/latest?symbols={string.Join(",", symbols)}";
    }

    /// <summary>
    ///   Concatenates all the parameter strings onto the base url with an ampersand ('&') in between each parameter.
    /// </summary>
    /// <param name="baseUrl">The first segment of the endpoint url including the symbols parameters</param>
    /// <param name="parameterStrings">All the other string parameters to add to the endpoint url </param>
    /// <returns>The endpoint url with all the parameters concatenated together</returns>
    private static string ConcatUrlParameters(string baseUrl, List<string> parameterStrings)
    {
        return $"{baseUrl}?{string.Join("&", parameterStrings)}";
    }
    
    /// <summary>
    ///   Creates the url for a historical type endpoint. The returned url is composed of a base url, start and end
    ///   DateTimes which all historical endpoints need, a limit of the maximum items per page response, the next page
    ///   token (if there is one), and any additional parameters that an endpoint may need.
    /// </summary>
    /// <param name="baseUrl">The base url with the specific data endpoint name</param>
    /// <param name="startTime">DateTime the historical quotes start at</param>
    /// <param name="endTime">DateTime the historical quotes will end at</param>
    /// <param name="nextPageToken">The token needed to request the next page or null if there isn't one</param>
    /// <param name="additionalParams">A list of additional parameters that the endpoint url requires</param>
    /// <returns>The constructed url for the specific endpoint with all the necessary parameters</returns>
    private static string HistoricalUrl(string baseUrl, DateTime startTime, DateTime endTime,
        string? nextPageToken, List<string>? additionalParams = null)
    {
        // make a new list with any additionalParams, add the time range and max items/ page params
        List<string> urlParams =
        [
            ..additionalParams ?? [],
            $"start={DateFormats.Url(startTime)}",
            $"end={DateFormats.Url(endTime)}",
            "limit=10000"
        ];

        // add the next page token if one was passed
        if (!string.IsNullOrEmpty(nextPageToken))
            urlParams.Add($"page_token={nextPageToken}");

        // return the url with the parameters concatenated to the base
        return ConcatUrlParameters(baseUrl, urlParams);
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
    public static string LatestBars(List<string> symbols) => LatestUrl("bars", symbols);

    /// <summary>
    ///   Get the latest bid and ask quotes for the specified stock ticker symbol(s).
    /// </summary>
    /// <param name="symbols">The ticker symbol(s) to get the latest quotes for</param>
    /// <returns>The api endpoint url for the latest quotes</returns>
    public static string LatestQuotes(List<string> symbols) => LatestUrl("quotes", symbols);

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
    /// <param name="nextPageToken">The token needed to request the next page, defaults to null</param>
    /// <returns>The api endpoint url for the next page of historical bars for the symbol</returns>
    public static string HistoricalBars(string symbol, string timeframe, DateTime startTime, DateTime endTime,
        string? nextPageToken = null)
    {
        // the bars endpoint url needs a symbol and timeframe so add them to a list in the url parameter format
        List<string> additionalParams = [$"symbols={symbol}", $"timeframe={timeframe}"];

        // pass the list into the HistoricalUrl method to include them in the url and return the result
        return HistoricalUrl($"{Data}/bars", startTime, endTime, nextPageToken, additionalParams);
    }

    /// <summary>
    ///   Builds the endpoint url for a single page of the historical quotes in the time from startTime to endTime for
    ///   the specified stock symbol.
    /// </summary>
    /// <param name="symbol">The ticker symbol to get the historical quotes for</param>
    /// <param name="startTime">DateTime the historical quotes start at</param>
    /// <param name="endTime">DateTime the historical quotes will end at</param>
    /// <param name="nextPageToken">The token needed to request the next page, defaults to null</param>
    /// <returns>The api endpoint url for the next page of historical quotes for the symbol</returns>
    public static string HistoricalQuotes(string symbol, DateTime startTime, DateTime endTime,
        string? nextPageToken = null)
    {
        // the quotes endpoint has no additional params, so just return the string result from HistoricalUrl 
        return HistoricalUrl($"{Data}/{symbol}/quotes", startTime, endTime, nextPageToken);
    }
}