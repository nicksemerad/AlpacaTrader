using Common;

namespace Component;

/// <summary>
///   Class that represents trades which are corresponding buy and sell orders. Trades will most likely have more
///   fields in the future when active (paper)trading portfolios are implemented.
/// </summary>
public class Trade
{
    /// <summary>
    ///   The stock symbol that the trade is for.
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    ///   The number of shares that were traded.
    /// </summary>
    public decimal Shares { get; set; }

    /// <summary>
    ///   The price the shares were bought at.
    /// </summary>
    public decimal BuyPrice { get; set; }

    /// <summary>
    ///   The time the shares were bought at.
    /// </summary>
    public DateTime BuyTimestamp { get; set; }

    /// <summary>
    ///   The price the shares were sold at.
    /// </summary>
    public decimal SellPrice { get; set; }

    /// <summary>
    ///   The time the shares were sold at.
    /// </summary>
    public DateTime SellTimestamp { get; set; }

    /// <summary>
    ///   The profit (or loss) per share for this trade.
    /// </summary>
    public decimal ShareProfit { get; set; }

    /// <summary>
    ///   The total profits (or losses) for all this trade's shares.
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    ///   A boolean that is true if the trade was a win, and false if it was a loss.
    /// </summary>
    public bool Win { get; set; }

    /// <summary>
    ///   Constructor for a new Trade. Uses a buy and sell order along with the number of matched shares to populate
    ///   the trade's properties.
    /// </summary>
    /// <param name="shares">The number of shares traded between the buy and sell orders</param>
    /// <param name="buyOrder">The buy order that bought this trade's shares</param>
    /// <param name="sellOrder">The sell order that sold this trade's shares</param>
    private Trade(decimal shares, Order buyOrder, Order sellOrder)
    {
        var profit = sellOrder.Price - buyOrder.Price;

        Symbol = buyOrder.Symbol;
        Shares = shares;
        BuyPrice = buyOrder.Price;
        BuyTimestamp = buyOrder.Timestamp;
        SellPrice = sellOrder.Price;
        SellTimestamp = sellOrder.Timestamp;
        ShareProfit = profit;
        TotalProfit = profit * shares;
        Win = profit > 0;
    }

    /// <summary>
    ///   Overrides object's ToString and returns a simple string representation of this trade.
    /// </summary>
    /// <returns>A string with basic trade details</returns>
    public override string ToString()
    {
        var wasWin = Win ? "WIN" : "LOSS";
        return $"[{wasWin}]: {Shares} shares BUY@${BuyPrice:N2} SELL@${SellPrice:N2} sell (${TotalProfit:N2})";
    }

    /// <summary>
    ///   Matches the buy and sell orders in a portfolio's historical orders list and returns a list of resulting
    ///   trades. A trade is created for each matched buy and sell order. A single sell order may be matched with
    ///   multiple buys and vice versa. This is useful because it allows the calculation of a strategy's average trade
    ///   profits, the percentage wins and losses, etc. Matches are made using a queue of buy orders and pairs them
    ///   with sell orders until all bought shares have been matched and sold. Each match is recorded as a trade.
    /// </summary>
    /// <returns>A list of the historical trades constructed from the provided order history</returns>
    public static List<Trade> MatchTrades(List<Order> orders)
    {
        // make a list of trades to return and a queue of buy orders for matching
        List<Trade> trades = [];
        Queue<Order> buyOrders = [];

        foreach (var order in orders)
        {
            // add the buy order to the queue
            if (order.Side == OrderSide.Buy)
            {
                buyOrders.Enqueue(order);
            }
            else
            {
                // it's a sell order so get the quantity of shares to sell
                var quantityToSell = order.Quantity;

                // while there are still shares to sell and unmatched buy orders
                while (quantityToSell > 0 && buyOrders.Count > 0)
                {
                    // get the first buy order and max shares sellable. add the trade to trades
                    var buy = buyOrders.Peek();
                    var quantitySellable = Math.Min(quantityToSell, buy.Quantity);
                    trades.Add(new Trade(quantitySellable, buy, order));

                    // update the remaining bought shares and quantity to sell
                    buy.Quantity -= quantitySellable;
                    quantityToSell -= quantitySellable;

                    // if the buy order's shares have all been sold, remove it from the queue
                    if (buy.Quantity == 0) buyOrders.Dequeue();
                }
            }
        }

        return trades;
    }
}