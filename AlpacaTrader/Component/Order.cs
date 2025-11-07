namespace Component;

using System.Text.Json.Serialization;
using Common;

/// <summary>
///   This class represents a buy or sell order.
/// </summary>
public class Order
{
    /// <summary>
    ///   This Order's unique ID.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }

    /// <summary>
    ///   The timestamp indicating when the order was created. 
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///   The symbol that this order is for. This field is
    ///   required when creating a new order via Alpaca API.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    /// <summary>
    ///   The side of this order. Either Buy or Sell. This field
    ///   is required when creating a new order via Alpaca API.
    /// </summary>
    [JsonPropertyName("side")]
    public OrderSide Side { get; set; }

    /// <summary>
    ///   The type of this order. Can be one of the 5 order types mentioned below.
    ///   <code>
    ///     [ "market", "limit", "stop", "stop_limit", "trailing_stop" ]
    ///   </code>
    ///   Different types of trading allow different order types. Equity trading allows all 5. Options and Multileg
    ///   Options allow only market and limit orders. Crypto allows those two types, in addition to stop limit orders.
    ///   This field is required when creating a new order via Alpaca API.
    /// </summary>
    [JsonPropertyName("type")]
    public OrderType OrderType { get; set; }

    /// <summary>
    ///   The conditions or length of time that this order is valid for. Can be one of the 6 options below.
    ///   <list type="bullet">
    ///     <item>day: good for the rest of the day, or is queued for the next day if sent after trading hours </item>
    ///     <item>gtc: good until cancelled</item>
    ///     <item>opg: only valid during market open, is cancelled after</item>
    ///     <item>cls: only valid during market close, is cancelled after</item>
    ///     <item>ioc: immediate or cancel, filled immediately or the order is cancelled, allows partial fills</item>
    ///     <item>foc: fill or kill, filled immediately and completely the order is cancelled</item>
    ///   </list>
    ///   This field is required when creating a new order via Alpaca API.
    /// </summary>
    [JsonPropertyName("time_in_force")]
    public TimeInForce TimeInForce { get; set; }

    /// <summary>
    ///   The quantity or number of shares that this order is for.
    /// </summary>
    [JsonPropertyName("qty")]
    public decimal Quantity { get; set; }

    /// <summary>
    ///   The quantity or number of shares that have been filled so far.
    /// </summary>
    [JsonPropertyName("filled_qty")]
    public decimal FilledQuantity { get; set; }

    /// <summary>
    ///   The dollar amount that this order is for. Can only use this if not using Quantity.
    /// </summary>
    [JsonPropertyName("notional")]
    public decimal Notional { get; set; }

    /// <summary>
    ///   The price that the order should be filled at, or better. This is needed for limit and stop limit orders.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public decimal LimitPrice { get; set; }

    /// <summary>
    ///   The price at which a trade is executed. This is needed for limit and stop limit orders.
    /// </summary>
    [JsonPropertyName("stop_price")]
    public decimal StopPrice { get; set; }

    /// <summary>
    ///   The Symbol's share price at the time of order creation.
    /// </summary>
    [JsonIgnore]
    public decimal Price { get; set; }

    /// <summary>
    ///   The share price * quantity at the time of order creation.
    /// </summary>
    [JsonIgnore]
    public decimal TotalCost { get; set; }
}