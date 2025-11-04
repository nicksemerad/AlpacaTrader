using Component;
using Newtonsoft.Json.Linq;

namespace Api;

/// <summary>
///   Handle parsing the json content that is returned from a request into its corresponding C# objects. This is done
///   using the Newtonsoft Json package, enabling the responses to be deserialized without needing custom classes to
///   deserialize them into.
/// </summary>
public class Response
{
    /// <summary>
    ///   The content string received from the request response. 
    /// </summary>
    private readonly string _content;

    /// <summary>
    ///   Constructs a new Response object with the provided content.
    /// </summary>
    /// <param name="content">The string content of a request.</param>
    public Response(string content)
    {
        _content = content;
    }

    /// <summary>
    ///   Try and get the root list of elements from _content and cast it to a List of type T. If this process is
    ///   successful, the out rootList parameter is set to the List of Ts and true is returned. If something went
    ///   wrong, rootList is set to null and false is returned.
    /// </summary>
    /// <param name="rootList">The List to set as the List of T parsed from the response json</param>
    /// <typeparam name="T">The type of elements in the root array that need to be parsed</typeparam>
    /// <returns>True if the parsing is successful, false if not</returns>
    private bool TryGetRootList<T>(out List<T>? rootList)
    {
        try
        {
            // convert the response content to a JArray, cast it to a List of T and return true
            var jArray = JArray.Parse(_content);
            rootList = jArray.ToObject<List<T>>();
            return true;
        }
        catch
        {
            rootList = null;
            return false;
        }
    }

    /// <summary>
    ///   Try and get the root object at the parameter key from _content and cast it to be of type T. If the object
    ///   is successfully retrieved and cast, return true. If anything fails, return false. The rootObject parameter
    ///   is set to the result of the cast if it is successful, or null if it fails.
    /// </summary>
    /// <param name="key">The key or name for the desired root object</param>
    /// <param name="rootObject">The root object of type T after it has been retrieved and cast, or null</param>
    /// <param name="isNullable">If the root object can be null without parsing issues, default false</param>
    /// <typeparam name="T">The type to try and cast the root object to</typeparam>
    /// <returns>True if the retrieval and cast of the object was successful, false if it wasn't</returns>
    private bool TryGetRootObject<T>(string key, out T? rootObject, bool isNullable = false)
    {
        try
        {
            // convert the response content to a JObject, then try to find and cast the object at "key" to a T
            var jObject = JObject.Parse(_content);
            rootObject = jObject[key]!.ToObject<T>();
            // return false if the rootObject is null and not nullable, else true
            return !(rootObject is null && !isNullable);
        }
        catch
        {
            rootObject = default;
            return false;
        }
    }

    /// <summary>
    ///   Private helper method to get the next_page_token from the response content json. If it is present in the json
    ///   it is returned, else an empty string is returned which will end pagination.
    /// </summary>
    /// <returns>The next page token if there is one, else an empty string</returns>
    private string GetNextPageToken()
    {
        // if the next_page_token is present in the json return it, else return an empty string
        return TryGetRootObject<string>("next_page_token", out var nextPageToken)
            ? nextPageToken!
            : string.Empty;
    }

    /// <summary>
    ///   Takes a symbol and JObject that has the quote data and converts it into a QuotePair.
    /// </summary>
    /// <param name="symbol">The stock symbol that the quote pair is for</param>
    /// <param name="jObject">The JObject that the quote data is stored in</param>
    /// <returns>The QuotePair object that was created using the JObject data</returns>
    private static QuotePair JObjectToQuotePair(string symbol, JObject jObject)
    {
        return new QuotePair(
            symbol,
            jObject.Value<DateTime?>("t") ?? default, // timestamp
            jObject.Value<string>("ax") ?? "", // ask exchange code
            jObject.Value<decimal?>("ap") ?? 0m, // ask price
            jObject.Value<double?>("as") ?? 0.0, // ask size
            jObject.Value<string>("bx") ?? "", // bid exchange code
            jObject.Value<decimal?>("bp") ?? 0m, // bid price
            jObject.Value<double?>("bs") ?? 0.0 // bid size
        );
    }

    /// <summary>
    ///   Parses the content response from a request for a single bar. (i.e. GetLatestBar, GetLatestBarsAsync)
    /// </summary>
    /// <returns>A list of the Bars parsed from the response.</returns>
    public List<Bar> ParseBars()
    {
        // try to get the bars from content as a string (symbol) and Bar dict, return an empty list if it fails
        if (!TryGetRootObject<Dictionary<string, Bar>>("bars", out var bars)) return [];

        // make and return a list of the bars after adding the stock symbol (key) to each bar (value)
        return bars!.Select(bar =>
        {
            bar.Value.Symbol = bar.Key;
            return bar.Value;
        }).ToList();
    }

    /// <summary>
    ///   Parses the json response for a single set of Quotes (only most recent bid and ask) for a list of symbols.
    ///   The response is turned into a QuotePair object which holds the symbol, Ask Quote, and Bid Quote.
    /// </summary>
    /// <returns>A list of Quotes, storing the most recent bid and ask for a number of symbols</returns>
    public List<QuotePair> ParseQuotes()
    {
        // try to get the quotes object from content as a JObject, return an empty list if it fails
        if (!TryGetRootObject<JObject>("quotes", out var quotes)) return [];

        // each property has the stock symbol as the name and quote data as the value, parse them to a list
        return quotes!.Properties().Select(prop => JObjectToQuotePair(prop.Name, (JObject)prop.Value)).ToList();
    }

    /// <summary>
    ///   Parses the content response from a request for historical symbol bars. (i.e. GetHistoricalBarsAsync)
    /// </summary>
    /// <param name="token">The token reference that points to the next page of Bars</param>
    /// <returns>A list of the bars parsed from the response content</returns>
    public List<Bar> ParseHistoricalBars(ref string token)
    {
        // try to get the bars as a dictionary of symbols and Bar lists, return an empty list if it fails
        if (!TryGetRootObject<Dictionary<string, List<Bar>>>("bars", out var symbolBars)) return [];

        // if the next_page_token is present in the json, update token, else set token as an empty string
        token = GetNextPageToken();

        // aggregate all the bars from each symbolBars element, cast it to a list and return it
        return symbolBars!.SelectMany(pair => pair.Value).ToList();
    }

    /// <summary>
    ///   Parses the content response from a request for a symbol's historical quotes.
    /// </summary>
    /// <param name="token">The token reference that points to the next page of Quotes</param>
    /// <returns>A list of the QuotePairs(Ask and Bid Quotes) parsed from the response content</returns>
    public List<QuotePair> ParseHistoricalQuotes(ref string token)
    {
        // try to get the quotes as a JArray and the symbol as a string, return an empty list if either fails
        if (!TryGetRootObject<JArray>("quotes", out var quotes)
            || !TryGetRootObject<string>("symbol", out var symbol)) return [];

        // if the next_page_token is present in the json, update token, else set token as an empty string
        token = GetNextPageToken();

        // use the symbol and each JArray quote data element to create and return a List of QuotePairs
        return quotes!.Select(quote => JObjectToQuotePair(symbol!, (JObject)quote)).ToList();
    }

    /// <summary>
    ///   Parses the content response from a request for the calendar of trading days into a List of CalendarDay.
    /// </summary>
    /// <returns>A List full of CalendarDay objects if parsing is successful, else an empty list</returns>
    public List<CalendarDay> ParseCalendarDays()
    {
        return TryGetRootList<CalendarDay>(out var calendarDays) ? calendarDays! : [];
    }
}