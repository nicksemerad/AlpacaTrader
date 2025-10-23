using System.Text.Json.Serialization;
using Common;

namespace Component;

public class Order
{
    public string Symbol { get; set; }
    
    [JsonPropertyName("qty")]
    public int Quantity { get; set; } // num shares to trade
    
    [JsonPropertyName("notional")]
    public int Notional { get; set; } // dollar amount to trade (cant do with qty)
    
    [JsonPropertyName("side")]
    public OrderSide Side { get; set; } // Buy or Sell
    
    [JsonPropertyName("type")]
    public OrderType OrderType { get; set; } // type or order
    // Equity trading: market, limit, stop, stop_limit, trailing_stop.
    // Options trading: market, limit.
    // Multileg Options trading: market, limit.
    // Crypto trading: market, limit, stop_limit.
    
    [JsonPropertyName("time_in_force")]
    public TimeInForce TimeInForce { get; set; } // how long the reade is valid for
    /*
     * day: rest of the day, or queued to start next day
     * gtc: good until cancelled
     * opg: only valid during open, cancelled after
     * cls: only valid during close
     * ioc: immediately or cancel
     * foc: fill or kill, either totally filled or cancelled
     */
    
    [JsonPropertyName("limit_price")]
    public int LimitPrice { get; set; } // needed for limit, stop_limit types
    
    [JsonPropertyName("stop_price")]
    public int StopPrice { get; set; }  // needed for limit, stop_limit types
}