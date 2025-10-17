using System.Text.Json;
using System.Text.Json.Serialization;
using Component;

namespace API;

/// <summary>
///   Handle parsing the content from a request by deserializing them into their response type. Using
///   The deserialized object the bars are extracted and returned. 
/// </summary>
public class Response
{
    /// <summary>
    ///   The content string recieved from the request response. 
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
    ///   Parses the content response from a request for a single bar. (i.e. GetLatestBar, GetLatestBars)
    /// </summary>
    /// <returns>A list of the Bars parsed from the response.</returns>
    public List<Bar> ParseBars()
    {
        // deserialize the response into a BarsResponse which has a list of Bars
        var barsResponse = JsonSerializer.Deserialize<BarsResponse>(_content);
        
        // return an empty list if bars is null
        if (barsResponse?.Bars == null)
            return [];
        
        // add the symbol to each of the bars before returning the list
        List<Bar> barsList = new List<Bar>();
        foreach (var (barSymbol, barData) in barsResponse.Bars)
        {
            barData.Symbol = barSymbol;
            barsList.Add(barData);
        }
        
        return barsList;
    }

    /// <summary>
    ///   Parses the content response from a request for historical symbol bars. (i.e. GetHistoricalBar)
    /// </summary>
    /// <param name="token">The token reference that points to the next page of Bars</param>
    /// <returns>A list of the bars parsed from the response content</returns>
    public List<Bar> ParseHistoricalBars(ref string token)
    {
        // deserialize into a HistoricalBarsResponse and make a list to hold the new bots
        var barsResponse = JsonSerializer.Deserialize<HistoricalBarsResponse>(_content);
        List<Bar> barsList = new List<Bar>();
        
        // return an empty list if bars is null
        if (barsResponse?.Bars == null)
            return [];
        
        // for each key symbol in barsResponse.Bars 
        foreach (var (symbol, bars) in barsResponse.Bars)
            // add the symbol to every bar in the list and add them to barsList
            foreach (var bar in bars)
            {
                bar.Symbol = symbol;
                barsList.Add(bar);
            }
        token = barsResponse.NextPageToken;

        // return the list of bars parsed from the response
        return barsList;
    }
    
    /// <summary>
    ///   For deserializing the response from GetLatestBar or GetLatestBars endpoints. The response json is all
    ///   contained in an object titled "bars". This object contains other objects, with each one being a bar with
    ///   its stock ticker symbol as the name. For example:
    ///   <code>
    ///     {
    ///       "bars" {
    ///         "TSLA": {
    ///           bar info
    ///         },
    ///         "AAPL": {
    ///           bar info
    ///         }
    ///       }
    ///     }
    ///   </code>
    /// </summary>
    private class BarsResponse
    {
        /// <summary>
        ///   A dictionary of strings as keys and Bars as values. This is what the "bars" json object
        ///   is deserialized into.
        /// </summary>
        [JsonPropertyName("bars")]
        public Dictionary<string, Bar> Bars { get; set; }
    }
    
    /// <summary>
    ///   For deserialization of responses from the historical bars endpoint. The response json has one object and
    ///   one string. All the bars information is contained in an object titled "bars". This object holds lists of
    ///   bars with the symbol as the list's title. The string is a long token that is used to paginate through the
    ///   symbol's historical bars. For example:
    ///   <code>
    ///     {
    ///       "bars" {
    ///         "TSLA": [
    ///           {
    ///             many bars
    ///           }
    ///         ],
    ///         "AAPL": [
    ///           many bars
    ///         ]
    ///       },
    ///       "next_page_token": random string"
    ///     }
    ///   </code>
    /// </summary>
    private class HistoricalBarsResponse
    {
        /// <summary>
        ///   A dictionary of strings as keys and Bars as values. This is what the "bars" json object is deserialized
        ///   into.
        /// </summary>
        [JsonPropertyName("bars")]
        public Dictionary<string, List<Bar>> Bars { get; set; }
        
        /// <summary>
        ///   A string holding the next page's token for pagination. This is what "next_page_token" deserializes into.
        /// </summary>
        [JsonPropertyName("next_page_token")]
        public string NextPageToken { get; set; }
    }
    
}
