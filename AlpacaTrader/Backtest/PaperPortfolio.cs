using RiskManager;

namespace Backtest;

using Component;
using Common;
using Microsoft.Extensions.Logging;

/// <summary>
///   Represents a paper portfolio that tracks all buy and sell orders, assets, and cash.
/// </summary>
public class PaperPortfolio
{
    /// <summary>
    ///   The logger for this PaperPortfolio.
    /// </summary>
    private static readonly ILogger PortLog = Logger.Create(nameof(PaperPortfolio));

    /// <summary>
    ///   Current cash.
    /// </summary>
    public decimal Cash { get; private set; }

    /// <summary>
    ///   The portfolio starting cash.
    /// </summary>
    public decimal InitialCash { get; private set; }

    /// <summary>
    ///   The portfolio's stock positions (symbol and quantity of shares).
    /// </summary>
    private Dictionary<string, decimal> Positions { get; }

    /// <summary>
    ///   The history of all buy and sell orders made.
    /// </summary>
    public List<Order> OrderHistory { get; }

    /// <summary>
    ///   The history of this portfolio's total value (shares * price + cash)
    /// </summary>
    public List<(DateTime date, decimal value)> ValueHistory { get; }

    /// <summary>
    ///   Creates a new paper portfolio with the specified starting cash.
    /// </summary>
    public PaperPortfolio(decimal initialCash)
    {
        Cash = initialCash;
        InitialCash = initialCash;
        Positions = [];
        OrderHistory = [];
        ValueHistory = [];
    }

    /// <summary>
    ///   Tries to execute a buy order for the symbol, price, and quantity of shares. If there isn't enough cash to
    ///   complete the order it returns false. If there is enough, the portfolio's cash, positions, and order history
    ///   are updated, and it returns true.
    /// </summary>
    /// <param name="symbol">Stock symbol to buy shares of</param>
    /// <param name="quantity">Number of shares to buy</param>
    /// <param name="price">Price per share</param>
    /// <param name="timestamp">Time of the buy order</param>
    /// <returns>True if successful, false if we can't afford it</returns>
    public bool TryBuy(string symbol, decimal quantity, decimal price, DateTime timestamp)
    {
        // add random slippage and calculate the total cost
        var res = 1 + TradingCosts.AddRandomSlippage(price);
        price *= res;
        
        
        var cost = quantity * price;

        // check if we can afford it before subtracting the cost
        if (Cash < cost) return false;
        Cash -= cost;

        // add the quantity just "bought" to Positions for the symbol
        Positions.TryAdd(symbol, 0m);
        Positions[symbol] += quantity;

        // make, log, and record the order
        var order = PaperOrder(timestamp, symbol, OrderSide.Buy, quantity, price, cost);
        PortLog.LogDebug("Order: {Order}", PaperOrderToString(order));
        OrderHistory.Add(order);

        return true;
    }

    /// <summary>
    ///   Tries to execute a sell order for the symbol, price, and quantity of shares. If it doesn't have enough shares
    ///   to meet the order quantity, it returns false. If there is enough, the portfolio's cash, positions, and order
    ///   history are updated, and it returns true.
    /// </summary>
    /// <param name="symbol">Stock symbol to sell shares of</param>
    /// <param name="quantity">Number of shares to sell</param>
    /// <param name="price">Price per share</param>
    /// <param name="timestamp">Time of the sell order</param>
    /// <returns>True if successful, false if we don't have enough shares</returns>
    public bool TrySell(string symbol, decimal quantity, decimal price, DateTime timestamp)
    {
        // make sure we have enough shares to sell
        if (!Positions.TryGetValue(symbol, out var shares) || shares < quantity)
            return false;
        
        // add random slippage to the price
        var res = 1 - TradingCosts.AddRandomSlippage(price);
        price *= res;

        // we have enough shares so consider them sold, update Cash and Positions
        var saleRevenue = quantity * price;
        Cash += saleRevenue;
        Positions[symbol] -= quantity;

        // make, log, and record the order
        var order = PaperOrder(timestamp, symbol, OrderSide.Sell, quantity, price, saleRevenue);
        PortLog.LogDebug("Order: {Order}", PaperOrderToString(order));
        OrderHistory.Add(order);

        return true;
    }

    /// <summary>
    ///   Calculates the current portfolio value (shares * share price + cash).
    /// </summary>
    /// <param name="latestBars">A list of the latest bar(s) for the symbol(s)</param>
    /// <returns>Total portfolio value</returns>
    private decimal GetPortfolioValue(List<Bar> latestBars)
    {
        // use the latest bars to calc each symbols current share price
        var currentPrices = latestBars.ToDictionary(bar => bar.Symbol, bar => bar.Close);

        // sum up all the shares and their prices in the portfolio
        decimal positionsValue = 0;
        foreach (var (symbol, quantity) in Positions)
            if (currentPrices.TryGetValue(symbol, out var price))
                positionsValue += quantity * price;

        return Cash + positionsValue;
    }

    /// <summary>
    ///   Gets the current quantity of shares held for a symbol.
    /// </summary>
    public decimal GetSymbolShares(string symbol)
    {
        return Positions.GetValueOrDefault(symbol, 0m);
    }

    /// <summary>
    ///   Records the current portfolio value using the list of the latest bars. The timestamp for the record is the
    ///   timestamp for the first bar in the list.
    /// </summary>
    /// <param name="latestBars">A list of the latest bar(s) for the symbol(s)</param>
    public void RecordValue(List<Bar> latestBars)
    {
        ValueHistory.Add((latestBars[0].Date, GetPortfolioValue(latestBars)));
    }

    /// <summary>
    ///   Creates a new order with the specified parameters.
    /// </summary>
    /// <param name="time">The timestamp when the order was created</param>
    /// <param name="symbol">The stock or asset symbol for the order</param>
    /// <param name="side">The side of the order, either Buy or Sell</param>
    /// <param name="quantity">The number of shares or units for the order</param>
    /// <param name="price">The price per unit of the asset involved in the order</param>
    /// <param name="total">The total cost or revenue of the order</param>
    /// <returns>A new order instance with the specified parameters</returns>
    private static Order PaperOrder(DateTime time, string symbol, OrderSide side, decimal quantity, decimal price,
        decimal total)
    {
        return new Order
        {
            Timestamp = time,
            Symbol = symbol,
            Side = side,
            Quantity = quantity,
            Price = price,
            TotalCost = total
        };
    }

    /// <summary>
    ///   Converts a paper order to a string that describes the order details.
    /// </summary>
    /// <param name="order">The order to be converted to string format</param>
    /// <returns>A formatted string representation of the specified order</returns>
    private static string PaperOrderToString(Order order)
    {
        return $"[{order.Timestamp:yyyy-MM-dd HH:mm}] {order.Side.DescString().ToUpper()} {order.Quantity:N} " +
               $"{order.Symbol} shares @ ${order.Price:N} ea. (${order.TotalCost:N})";
    }
}