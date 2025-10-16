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
    private string _content;
    
    public Response(string content)
    {
        _content = content;
    }
    
    /// <summary>
    ///   For a single bar
    /// </summary>
    /// <returns></returns>
    public List<Bar> ParseBars()
    {
        var barsResponse = JsonSerializer.Deserialize<BarsResponse>(_content);
        List<Bar> barsList = barsResponse.Bars.Select(kvp => 
        {
            Bar bar = kvp.Value;
            bar.Symbol = kvp.Key;
            return bar;
        }).ToList();
        return barsList;
    }

    /// <summary>
    ///   For historical bars
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public List<Bar> ParseHistoricalBars(out string token)
    {
        var barsResponse = JsonSerializer.Deserialize<HistoricalBarsResponse>(_content);
        List<Bar> barsList = new List<Bar>();
        
        // add the bars from the deserialized json to bars, and set the next page token
        foreach (var bar in barsResponse.Bars)
            barsList.AddRange(bar.Value);
        token = barsResponse.NextPageToken;
        
        return barsList;
    }
    
    /// <summary>
    ///   For deserialization of responses from the latest bars endpoint.
    /// </summary>
    private class BarsResponse
    {
        [JsonPropertyName("bars")]
        public Dictionary<string, Bar> Bars { get; set; }
    }
    
    /// <summary>
    ///   For deserialization of responses from the historical bars endpoint.
    /// </summary>
    private class HistoricalBarsResponse
    {
        [JsonPropertyName("bars")]
        public Dictionary<string, List<Bar>> Bars { get; set; }
        
        [JsonPropertyName("next_page_token")]
        public string NextPageToken { get; set; }
    }
    
}
