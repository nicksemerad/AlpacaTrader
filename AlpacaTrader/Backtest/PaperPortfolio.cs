using Common;

namespace Backtest;

/// <summary>
///   Represents a paper portfolio that tracks all buy and sell orders, assets, and cash.
/// </summary>
public class PaperPortfolio
{
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
    public Dictionary<string, decimal> Positions { get; }

    /// <summary>
    ///   The history of all buy and sell orders made.
    /// </summary>
    public List<PaperOrder> OrderHistory { get; }

    /// <summary>
    ///   Creates a new paper portfolio with the specified starting cash.
    /// </summary>
    public PaperPortfolio(decimal initialCash)
    {
        Cash = initialCash;
        InitialCash = initialCash;
        Positions = new Dictionary<string, decimal>();
        OrderHistory = new List<PaperOrder>();
    }

    /// <summary>
    ///   Executes a buy order.
    /// </summary>
    /// <param name="symbol">Stock symbol to buy</param>
    /// <param name="quantity">Number of shares to buy</param>
    /// <param name="price">Price per share</param>
    /// <param name="timestamp">Time of the buy order</param>
    /// <returns>True if successful, false if we can't afford it</returns>
    public bool Buy(string symbol, decimal quantity, decimal price, DateTime timestamp)
    {
        // calculate the total cost
        var cost = quantity * price;

        // check if we can afford it
        if (cost > Cash)
            return false;

        // we can afford it, so subtract the cost
        Cash -= cost;

        // add the quantity just "bought" to Positions for the symbol
        if (!Positions.ContainsKey(symbol))
            Positions[symbol] = 0m;
        Positions[symbol] += quantity;

        // record the buy order
        OrderHistory.Add(new PaperOrder(timestamp, symbol, OrderSide.Buy, quantity, price, cost));

        return true;
    }

    /// <summary>
    ///   Executes a sell order.
    /// </summary>
    /// <param name="symbol">Stock symbol to sell</param>
    /// <param name="quantity">Number of shares to sell</param>
    /// <param name="price">Price per share</param>
    /// <param name="timestamp">Time of the sell order</param>
    /// <returns>True if successful, false if we don't have enough shares</returns>
    public bool Sell(string symbol, decimal quantity, decimal price, DateTime timestamp)
    {
        // make sure we have enough shares to sell
        if (!Positions.TryGetValue(symbol, out decimal shares) || shares < quantity)
            return false;

        // sell successful, update Cash and Positions
        var saleRevenue = quantity * price;
        Cash += saleRevenue;
        Positions[symbol] -= quantity;

        // if we sold all our shares, remove the symbol from Positions
        if (Positions[symbol] == 0m)
            Positions.Remove(symbol);

        // record the sell order
        OrderHistory.Add(new PaperOrder(timestamp, symbol, OrderSide.Sell, quantity, price, saleRevenue));

        return true;
    }

    /// <summary>
    ///   Calculates the current portfolio value (cash + position values).
    /// </summary>
    /// <param name="currentPrices">Current prices for each held symbol</param>
    /// <returns>Total portfolio value</returns>
    public decimal GetPortfolioValue(Dictionary<string, decimal> currentPrices)
    {
        // sum up all the shares and their prices in the portfolio
        decimal positionValue = 0;
        foreach (var (symbol, quantity) in Positions)
            if (currentPrices.TryGetValue(symbol, out var quantityValue))
                positionValue += quantity * quantityValue;

        return Cash + positionValue;
    }

    /// <summary>
    ///   Gets the current quantity of shares held for a symbol.
    /// </summary>
    public decimal GetSymbolShares(string symbol)
    {
        return Positions.GetValueOrDefault(symbol, 0m);
    }
}

/// <summary>
///   A single buy or sell order.
/// </summary>
public class PaperOrder(DateTime time, string symbol, OrderSide side, decimal quantity, decimal price, decimal total)
{
    public DateTime Timestamp { get; } = time;
    public string Symbol { get; } = symbol;
    public OrderSide Side { get; } = side;
    public decimal Quantity { get; } = quantity;
    public decimal Price { get; } = price;
    public decimal Total { get; } = total;


    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm}] {Side.ToDescription()} {Quantity} {Symbol} shares " +
               $"@ ${Price:F2} => ${Total:F2};";
    }
}