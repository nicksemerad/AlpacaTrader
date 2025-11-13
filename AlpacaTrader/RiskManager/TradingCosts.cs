namespace RiskManager;

using Component;

/// <summary>
///   https://files.alpaca.markets/disclosures/library/BrokFeeSched.pdf
///   https://docs.alpaca.markets/docs/regulatory-fees
///
///   At the end of each day the total fees will be charged to the account, rounded to the nearest penny.
/// </summary>
public static class TradingCosts
{
    private const decimal Finra = 0.000166m; // max of $8.30 per order
    private const decimal Cat = 0.0000265m;
    private const decimal Commission = 0.00m; // currently no commission fees from alpaca
    private const decimal AlpacaMarginInterest = 0.065m; // annualized, not using currently

    /// <summary>
    ///   Estimate the total fees for a list of orders, taking the FINRA and CAT fees into consideration along with the
    ///   commission fees. Currently, there are no commission fees, but this method will still work if they are added
    ///   in the future. At the end of each day, Alpaca rounds the total fees up to the nearest penny and charges it
    ///   to the trading account. This is simulated by grouping the order history by day and then summing the rounded
    ///   total fees for each day.
    /// </summary>
    /// <param name="orders">The list of orders to estimate the total fees for</param>
    /// <returns>The estimated total fees that Alpaca would charge</returns>
    public static decimal EstimateOrderHistoryTotalFees(List<Order> orders)
    {
        // group orders by day, as that is when fees are rounded and charged
        var dayOrders = orders.GroupBy(o => DateOnly.FromDateTime(o.Timestamp)).ToList();

        var totalShares = orders.Sum(o => o.Quantity);
        Console.WriteLine(totalShares);

        // first time seeing this syntax thanks to my IDE recommending it.
        return (from day in dayOrders
            let dayShares = day.Sum(d => d.Quantity) // sum the shares for each day
            let dayFinraFees = dayShares < 50_000 ? RoundToNearestPenny(dayShares * Finra) : 8.30m
            let dayCatFees = dayShares * Cat
            let dayCommissions = day.Sum(d => d.Quantity * d.Price) * Commission
            select dayFinraFees + dayCatFees + dayCommissions // add the fees and commission
            into dailyTotalFees // round the total fees to the nearest penny
            select RoundToNearestPenny(dailyTotalFees)).Sum();
    }

    /// <summary>
    ///   Rounds a decimal value up to the nearest penny.
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <returns>The decimal value rounded up to the nearest penny</returns>
    private static decimal RoundToNearestPenny(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    ///   Adds random slippage to a price that is +/- 0.25% of the price, or whichever maxSlippage factor is passed in.
    ///   The resulting price is rounded to the nearest penny.
    /// </summary>
    /// <param name="maxSlippage">The maximum +/- slippage range</param>
    /// <returns>The share price after applying a random slippage factor in the specified range</returns>
    public static decimal GetRandomSlippage(decimal maxSlippage = 0.0025m)
    {
        var randomDouble = new Random().NextDouble();
        return (decimal)randomDouble * maxSlippage;
    }
}