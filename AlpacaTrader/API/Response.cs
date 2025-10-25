using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
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
        // parse the json into a JObject, and the bars attribute to a dict with the symbol and bar
        JObject json = JObject.Parse(_content);
        Dictionary<string, Bar> bars = json["bars"]!.ToObject<Dictionary<string, Bar>>() ?? new();

        // make a list of the bars after adding the stock symbol to each bar
        List<Bar> barsList = bars.Select(bar =>
        {
            bar.Value.Symbol = bar.Key;
            return bar.Value;
        }).ToList();

        return barsList;
    }

    /// <summary>
    ///   Parses the content response from a request for historical symbol bars. (i.e. GetHistoricalBar)
    /// </summary>
    /// <param name="token">The token reference that points to the next page of Bars</param>
    /// <returns>A list of the bars parsed from the response content</returns>
    public List<Bar> ParseHistoricalBars(ref string token)
    {
        // deserialize into a HistoricalBarsResponse and make a list to hold the new bars
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
    ///   Parses the content response from a request for a symbol's historical quotes. (i.e. GetHistoricalQuotes)
    /// </summary>
    /// <param name="token">The token reference that points to the next page of Quotes</param>
    /// <returns>A list of the QuotePairs(Ask and Bid Quotes) parsed from the response content</returns>
    public List<QuotePair> ParseHistoricalQuotes(ref string token)
    {
        // deserialize into a HistoricalQuotesResponse and make a list to hold the new QuotePairs
        var quotesResponse = JsonSerializer.Deserialize<HistoricalQuotesResponse>(_content);
        
        // return an empty list if Quotes is null
        if (quotesResponse?.Quotes == null) return [];

        // update the next page token
        token = quotesResponse.NextPageToken;
        
        // make a list to hold each symbol's Ask and Bid quote pairs
        List<QuotePair> quotesList = new List<QuotePair>();

        // create a new QuotePair from every QuoteResponse object in the response Quotes
        foreach (QuoteResponse quote in quotesResponse.Quotes)
        {
            quotesList.Add(new QuotePair(quotesResponse.Symbol, quote.Timestamp, quote.AskExchange, quote.AskPrice, 
                quote.AskSize, quote.BidExchange, quote.BidPrice, quote.BidSize));
        }
        
        return quotesList;
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
        [JsonPropertyName("as")] public double AskSize { get; set; }

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
        [JsonPropertyName("bs")] public double BidSize { get; set; }
        
        /// <summary>
        ///   A list of codes representing the quote conditions.
        /// </summary>
        [JsonPropertyName("c")] public List<string> Conditions { get; set; }

        /// <summary>
        ///   The tape code of the quotes i.e. "A"=NYSE "B"=OTHER "C"=NASDAQ
        /// </summary>
        [JsonPropertyName("z")] public string Tape { get; set; }
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
    ///   For deserialization of responses from the historical quotes endpoint. The response json has three elements:
    ///   a list of quote pairs called quotes, a next_page_token string used for pagination, and a symbol string for
    ///   the stock symbol that the quotes are for. For example:
    ///   <code>
    ///     { "quotes" [ many quotes ], "next_page_token": random string, "symbol": stock symbol }
    ///   </code>
    /// </summary>
    private class HistoricalQuotesResponse
    {
        /// <summary>
        ///   The list of historical quotes for the stock symbol.
        /// </summary>
        [JsonPropertyName("quotes")] public List<QuoteResponse> Quotes { get; set; }
        
        /// <summary>
        ///   A string holding the next page's token for pagination. This is what "next_page_token" deserializes into.
        /// </summary>
        [JsonPropertyName("next_page_token")] public string NextPageToken { get; set; }
        
        /// <summary>
        ///   The symbol that these historical quotes are for.
        /// </summary>
        [JsonPropertyName("symbol")] public string Symbol { get; set; }
    }
}