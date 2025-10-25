using System.Text.Json;
using System.Text.Json.Serialization;
using Common;
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
        if (barsResponse?.Bars == null) return [];

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

        // return an empty list if bars is null
        if (barsResponse?.Bars == null) return [];
        
        // update the next page token
        token = barsResponse.NextPageToken;

        // for each key symbol in barsResponse.Bars 
        List<Bar> barsList = new List<Bar>();
        foreach (var (symbol, bars) in barsResponse.Bars)
        {
            // add the symbol to every bar in the list and add them to barsList
            foreach (Bar bar in bars)
            {
                bar.Symbol = symbol;
                barsList.Add(bar);
            }
        }
        
        // return the list of bars parsed from the response
        return barsList;
    }

    /// <summary>
    ///   Parses the json response for a single set of Quotes (only most recent bid and ask) for a list of symbols.
    ///   The response is turned into a QuotePair object which holds the symbol, Ask Quote, and Bid Quote.
    /// </summary>
    /// <returns>A list of Quotes, storing the most recent bid and ask for a number of symbols</returns>
    public List<QuotePair> ParseQuotes()
    {
        // deserialize the response into a QuotesResponse which has a dictionary of symbols and quotes
        var quotesResponse = JsonSerializer.Deserialize<QuotesResponse>(_content);

        // return an empty list if Quotes is null
        if (quotesResponse?.Quotes == null) return [];

        // make a list to hold each symbol's Bid quote and Ask quote, and add all the quotes in Quotes to it
        List<QuotePair> quotesList = new List<QuotePair>();
        foreach (var (symbol, data) in quotesResponse.Quotes)
            quotesList.Add(new QuotePair(symbol, data.Timestamp, data.AskExchange, data.AskPrice, data.AskSize,
                data.BidExchange, data.BidPrice, data.BidSize));

        return quotesList;
    }

    /// <summary>
    ///   For deserializing the response from GetLatestBar or GetLatestBars endpoints. The response json is all
    ///   contained in an object titled "bars". This object contains other objects, with each one being a bar with
    ///   its stock ticker symbol as the name. For example:
    ///   <code>
    ///     { "bars" { "TSLA": { bar info }, "AAPL": { bar info } } }
    ///   </code>
    /// </summary>
    private class BarsResponse
    {
        /// <summary>
        ///   A dictionary of strings as keys and Bars as values. This is what the "bars" json object
        ///   is deserialized into.
        /// </summary>
        [JsonPropertyName("bars")] public Dictionary<string, Bar> Bars { get; set; }
    }

    /// <summary>
    ///   For deserialization of responses from the historical bars endpoint. The response json has one object and
    ///   one string. All the bars information is contained in an object titled "bars". This object holds lists of
    ///   bars with the symbol as the list's title. The string is a long token that is used to paginate through the
    ///   symbol's historical bars. For example:
    ///   <code>
    ///     {
    ///       "bars" { "TSLA": [ { many bars } ], "AAPL": [ many bars ] },
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
        [JsonPropertyName("bars")] public Dictionary<string, List<Bar>> Bars { get; set; }

        /// <summary>
        ///   A string holding the next page's token for pagination. This is what "next_page_token" deserializes into.
        /// </summary>
        [JsonPropertyName("next_page_token")] public string NextPageToken { get; set; }
    }

    /// <summary>
    ///   For deserializing the response from GetLatestQuotes endpoint. The response json has one object called quotes.
    ///   This object can have one or many objects, with each one being titled a stock ticker. Each stock ticker object
    ///   holds all the data for the most recent update to the stock's ask or bid quotes. For example:
    ///   <code>
    ///     { "quotes" { "AAPL": { quotes info }, "TSLA": { quotes info } } }
    ///   </code>
    /// </summary>
    private class QuotesResponse
    {
        /// <summary>
        ///   Dictionary storing pairs of a stock symbol and most recent quote response.
        /// </summary>
        [JsonPropertyName("quotes")]  public Dictionary<string, QuoteResponse> Quotes { get; set; }
    }

    /// <summary>
    ///   For deserializing a single quote object from a json response from GetLatestQuotes or GetHistoricalQuotes
    ///   endpoints. The response json has 9 different data points for a stock's most recent quotes update, including
    ///   the exchange, price, and size for the most recent bid and ask, as well as some metadata, each being explained
    ///   in more detail in the class attributes. For example:
    ///   <code>
    ///     { "t": time, "ax": ask exchange, "ap": ask price, "as": ask price, "bx": bid exchange,
    ///       "bp": bid price, "bs": bid size, "c": [ conditions ], "z": tape }
    ///   </code>
    /// </summary>
    private class QuoteResponse
    {
        /// <summary>
        ///   Timestamp for the quotes.
        /// </summary>
        [JsonPropertyName("t")] public DateTime Timestamp { get; set; }

        /// <summary>
        ///   An exchange code representing which exchange the ask came from i.e. "N"=NYSE "V"=IEX
        /// </summary>
        [JsonPropertyName("ax")] public string AskExchange { get; set; }

        /// <summary>
        ///   The price for the ask.
        /// </summary>
        [JsonPropertyName("ap")] public decimal AskPrice { get; set; }

        /// <summary>
        ///   The number of shares for the ask.
        /// </summary>
        [JsonPropertyName("as")] public int AskSize { get; set; }

        /// <summary>
        ///   An exchange code representing which exchange the bid came from i.e. "N"=NYSE "V"=IEX
        /// </summary>
        [JsonPropertyName("bx")] public string BidExchange { get; set; }

        /// <summary>
        ///   The price for the bid.
        /// </summary>
        [JsonPropertyName("bp")] public decimal BidPrice { get; set; }

        /// <summary>
        ///   The number of shares for the bid.
        /// </summary>
        [JsonPropertyName("bs")] public int BidSize { get; set; }
        
        /// <summary>
        ///   A list of codes representing the quote conditions.
        /// </summary>
        [JsonPropertyName("c")] public List<string> Conditions { get; set; }

        /// <summary>
        ///   The tape code of the quotes i.e. "A"=NYSE "B"=OTHER "C"=NASDAQ
        /// </summary>
        [JsonPropertyName("z")] public string Tape { get; set; }
    }
}