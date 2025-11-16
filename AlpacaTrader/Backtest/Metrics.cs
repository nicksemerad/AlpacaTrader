namespace Backtest;

using Common;
using Component;
using Strategy;
using Microsoft.Extensions.Logging;

/// <summary>
///   Handles the calculation of metrics for paper portfolio backtests. May be moved/ changed in the future to be used
///   for paper or live trading.
/// </summary>
public static class Metrics
{
    /// <summary>
    ///   The logger for this Metrics class.
    /// </summary>
    private static readonly ILogger MetricsLog = Logger.Create(nameof(BacktestEngine));

    /// <summary>
    ///   Logs the starting conditions for a backtest. This includes the symbol, initial cash, total bars, and time
    ///   period.
    /// </summary>
    /// <param name="totalBars">All the bars that are being used to backtest a strategy</param>
    /// <param name="portfolio">The portfolio being used to track the backtest's results</param>
    public static void LogBacktestStart(List<Bar> totalBars, PaperPortfolio portfolio)
    {
        var dateRange = $"{totalBars.First().Date:yyyy-MM-dd} -> {totalBars.Last().Date:yyyy-MM-dd}";
        // Console.WriteLine($"DAYS: {totalBars.GroupBy(b => DateOnly.FromDateTime(b.Date)).Count()}");

        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("        BACKTEST:  START");
        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("          Symbol:  {Symbol}", totalBars[0].Symbol);
        MetricsLog.LogInformation("    Initial cash:  ${Cash:N}", portfolio.InitialCash);
        MetricsLog.LogInformation("      Total bars:  {Bars:N0}", totalBars.Count);
        MetricsLog.LogInformation("     Time period:  {Period}", dateRange);
        MetricsLog.LogInformation("");
    }

    /// <summary>
    ///   Logs the performance metrics for a paper portfolio after it has been backtested. There are a number of
    ///   metrics, I decided to not add comments for them as they are mostly self-explanatory. There are a number of
    ///   other metrics I plan to add in the future, like the Sharpe/ Sortino ratios.
    /// </summary>
    /// <param name="portfolio">The portfolio to calculate and log the metrics for</param>
    public static void LogBacktestResults(PaperPortfolio portfolio)
    {
        // make sure the portfolio has some history to log metrics for
        if (portfolio.OrderHistory.Count == 0 || portfolio.ValueHistory.Count == 0)
            throw new InvalidOperationException("Cannot log metrics for an empty portfolio history");

        var startEquity = portfolio.InitialCash;
        var fees = TradingCosts.EstimateOrderHistoryTotalFees(portfolio.OrderHistory);
        var endEquity = portfolio.ValueHistory.Last().value - fees;
        var maxEquity = portfolio.ValueHistory.Max(v => v.value);

        var sharesTraded = portfolio.OrderHistory.Sum(o => o.Quantity);
        var pnl = endEquity - startEquity;
        var roi = pnl / startEquity;
        var sharpe = CalcSharpeRatio(portfolio);
        var maxDrawdown = maxEquity - endEquity;
        var maxDrawdownPercent = maxDrawdown / endEquity;

        var allTrades = Trade.MatchTrades(portfolio.OrderHistory);
        var wins = allTrades.Where(t => t.Win).ToList();
        var losses = allTrades.Where(t => !t.Win).ToList();

        var winRate = (decimal)wins.Count / allTrades.Count;
        var avgWin = wins.Average(w => w.TotalProfit);
        var avgLoss = losses.Average(l => l.TotalProfit);
        var maxLossStreak = CalculateMaxLossStreak(allTrades);

        // log all the calculated metrics from the backtested portfolio
        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("        BACKTEST:  RESULTS");
        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("    Initial Cash:  ${StartEq:N}", startEquity);
        MetricsLog.LogInformation("    Final Equity:  ${EndEq:N}", endEquity);
        MetricsLog.LogInformation("      Max Equity:  ${MaxEq:N}", maxEquity);
        MetricsLog.LogInformation("        Est.Fees:  ${Fees:N}", fees);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("   Shares Traded:  {Shares:N0}", sharesTraded);
        MetricsLog.LogInformation("             PnL:  ${PnL:N}", pnl);
        MetricsLog.LogInformation("             ROI:  {Roi:P2}", roi);
        MetricsLog.LogInformation("    Max Drawdown:  ${MaxDrawdown:N}", maxDrawdown);
        MetricsLog.LogInformation("  Max Drawdown %:  {MaxDrawdownPercent:P2}", maxDrawdownPercent);
        MetricsLog.LogInformation("    Sharpe Ratio:  {Sharpe:N2}", sharpe);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("    Total Orders:  {NumOrders:N0}", portfolio.OrderHistory.Count);
        MetricsLog.LogInformation("    Total Trades:  {NumTrades:N0}", allTrades.Count);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("        Win Rate:  {WinRate:P2}", winRate);
        MetricsLog.LogInformation("  Winning Trades:  {WinTrades:N0}", wins.Count);
        MetricsLog.LogInformation("        Avg. Win:  ${AvgTrade:N}", avgWin);
        MetricsLog.LogInformation("   Losing Trades:  {LoseTrades:N0}", losses.Count);
        MetricsLog.LogInformation("       Avg. Loss:  ${AvgTrade:N}", avgLoss);
        MetricsLog.LogInformation(" Max Loss Streak:  {MaxConLoss:N0}", maxLossStreak);
    }

    /// <summary>
    ///   Calculates the maximum consecutive loss streak for the provided list of trades.
    /// </summary>
    /// <param name="trades">The list of the portfolio's trades</param>
    /// <returns>An integer with the max loss streak</returns>
    private static int CalculateMaxLossStreak(List<Trade> trades)
    {
        // set the max and current streaks/ values to zero before starting
        int lossStreak = 0, maxLossStreak = 0;

        foreach (var trade in trades)
        {
            // if the trade was a loss, update the current (and max if needed) streak, else restart it
            if (!trade.Win)
            {
                if (++lossStreak > maxLossStreak) maxLossStreak = lossStreak;
            }
            else
            {
                lossStreak = 0;
            }
        }

        return maxLossStreak;
    }


    /// <summary>
    ///   Calculates the Sharpe ratio using the portfolio's value history. The Sharpe ratio is a metric that tries to
    ///   measure the risk-adjusted return of a portfolio. The annualized parameter is used to determine whether the
    ///   user wants the annualized sharpe (defaults to true because it is most common), or if they want the daily
    ///   sharpe. Note that this assumes that the risk-free rate is 2%, which seems like a low estimate from what I've
    ///   seen online.
    /// </summary>
    /// <param name="portfolio">The portfolio with the value history to calculate the Sharpe Ratio of</param>
    /// <param name="annualized">If the user wants the annualized Sharpe instead of daily, defaults to true</param>
    /// <returns>The portfolio Sharpe Ratio as a double</returns>
    private static double CalcSharpeRatio(PaperPortfolio portfolio, bool annualized = true)
    {
        // calculate the daily return percentages for the portfolio's history
        var returns = CalcDailyReturns(portfolio);

        // need at least 2 values to calc the standard deviation
        if (returns.Count < 2) return 0;

        // calculate the mean and standard deviation of the daily returns
        var mean = returns.Average();
        var sumOfSquares = returns.Sum(r => Math.Pow(r - mean, 2));
        var stdDev = Math.Sqrt(sumOfSquares / (returns.Count - 1));

        // make sure the standard deviation isn't zero before dividing
        if (stdDev == 0) return 0;

        // assume that the risk-free rate is 2% and calculate the daily risk-free rate
        var riskFreeRate = 0.02;
        var rfDaily = Math.Pow(1 + riskFreeRate, 1 / 252d) - 1;

        // calculate the Sharpe ratio and annualize it by default
        var sharpe = (mean - rfDaily) / stdDev;
        return annualized ? sharpe * Math.Sqrt(252) : sharpe;
    }

    /// <summary>
    ///   Calculates the return percentage for each day recorded in the portfolio's value history.
    /// </summary>
    /// <param name="portfolio">The portfolio with the value history to calculate the daily returns of</param>
    /// <returns>A list of doubles with each one corresponding to a day's return percentage</returns>
    private static List<double> CalcDailyReturns(PaperPortfolio portfolio)
    {
        // group the portfolio's value history by date and get each day's close value
        var values = portfolio.ValueHistory
            .GroupBy(v => DateOnly.FromDateTime(v.date))
            .Select(g => g.Last().value)
            .ToList();

        // list to hold the daily return percentages
        List<double> dailyReturns = [];

        // calculate the daily return for each day and add it to the list
        for (int i = 1; i < values.Count; i++)
        {
            // divide the difference between today and yesterday's close values by yesterday's close value
            var dayReturns = (values[i] - values[i - 1]) / values[i - 1];
            dailyReturns.Add((double)dayReturns);
        }

        return dailyReturns;
    }
}