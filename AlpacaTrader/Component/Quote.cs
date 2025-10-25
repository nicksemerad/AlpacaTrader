using Common;

namespace Component;

/// <summary>
///   This class represents a stock symbol quote including the
///   timestamp, side (bid or ask), the exchange, price, and size.
/// </summary>
public class Quote
{
    /// <summary>
    ///   The timestamp that the quote was made.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///   The side of the quote i.e. ask or bid.
    /// </summary>
    public QuoteSide Side { get; set; } // ask or bid

    /// <summary>
    ///   An exchange code representing which exchange the quote came from i.e. "N"=NYSE "V"=IEX
    /// </summary>
    public string Exchange { get; set; }

    /// <summary>
    ///   The price the quote was made with.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    ///   The number of shares the quote was made for.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    ///   Creates a new Quote object with the parameters.
    /// </summary>
    /// <param name="timestamp">The quote timestamp</param>
    /// <param name="side">The side of the quote as a QuoteSide enum (bid or ask)</param>
    /// <param name="exchange">A code for the exchange the quote originates from</param>
    /// <param name="price">The quoted price</param>
    /// <param name="size">The number of shares the quote is for</param>
    public Quote(DateTime timestamp, QuoteSide side, string exchange, decimal price, int size)
    {
        Timestamp = timestamp;
        Side = side;
        Exchange = exchange;
        Price = price;
        Size = size;
    }

    /// <summary>
    ///   Overrides object's ToString to make printing and viewing a quote easier.
    /// </summary>
    /// <returns>A string holding the quote side, exchange code, price, and size</returns>
    public override string ToString()
    {
        return $"{Side.ToDescription().ToUpper()}: " +
               $"{Size} shares for ${Price} ea. via {Exchange}";
    }
}

/// <summary>
///   This class represents a pair of bid and ask quotes for a stock symbol made at one time.
/// </summary>
public class QuotePair
{
    /// <summary>
    ///   The stock symbol that the bid and ask quotes are for.
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    ///   The symbol's ask quote.
    /// </summary>
    public Quote? Ask { get; set; }

    /// <summary>
    ///   The symbol's bid quote.
    /// </summary>
    public Quote? Bid { get; set; }

    /// <summary>
    ///   Creates a new Quotes object by using the parameters to create a bid Quote and Ask Quote for the symbol. It is
    ///   possible for a quotes response to only have data for the bid or only for the ask. This happens if one of the
    ///   two is sent out at a certain timestamp, while one of the other type was not. If this happens the missing
    ///   ask or bid quote will be set to null.
    /// </summary>
    /// <param name="symbol">The stock symbol that the quotes are for</param>
    /// <param name="timestamp">The time that the quotes were made</param>
    /// <param name="askExchange">The code for the exchange the ask quote originated from</param>
    /// <param name="askPrice">The price listed in the ask quote</param>
    /// <param name="askSize">The number of shares in the ask quote</param>
    /// <param name="bidExchange">The code for the exchange the bid quote originated from</param>
    /// <param name="bidPrice">The price listed in the bid quote</param>
    /// <param name="bidSize">The number of shares in the bid quote</param>
    public QuotePair(string symbol, DateTime timestamp, string askExchange, decimal askPrice, int askSize,
        string bidExchange, decimal bidPrice, int bidSize)
    {
        Symbol = symbol;
        // if the quotes response didn't include the ask data then the Ask quote is null
        Ask = string.IsNullOrWhiteSpace(askExchange)
            ? null
            : new Quote(timestamp, QuoteSide.Ask, askExchange, askPrice, askSize);
        // if the bid exchange wasn't included in the response then the Bid quote is null
        Bid = string.IsNullOrWhiteSpace(bidExchange)
            ? null
            : new Quote(timestamp, QuoteSide.Bid, bidExchange, bidPrice, bidSize);
    }

    /// <summary>
    ///   Overrides object's ToString to make printing and viewing a pair of ask and bid quotes together easier.
    /// </summary>
    /// <returns>A string holding the symbol, ask quote, and bid quote</returns>
    public override string ToString()
    {
        return $"Quotes for {Symbol}: \n\t {Ask?.ToString() ?? "None"} \n\t {Bid?.ToString() ?? "None"}";
    }
}