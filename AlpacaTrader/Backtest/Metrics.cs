namespace Backtest;

using Common;
using Component;
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

        var startVal = portfolio.InitialCash;
        var endVal = portfolio.ValueHistory.Last().value;

        var pnl = endVal - startVal;
        var totalReturn = pnl / startVal;

        var allTrades = Trade.MatchTrades(portfolio.OrderHistory);
        var wins = allTrades.Where(t => t.Win).ToList();
        var losses = allTrades.Where(t => !t.Win).ToList();

        var winRate = (decimal)wins.Count / allTrades.Count;
        var avgWin = wins.Average(w => w.TotalProfit);
        var avgLoss = losses.Average(l => l.TotalProfit);

        var (maxLossStreak, maxDrawdown) = CalculateMaxLossStreakAndDrawdown(allTrades);

        // log all the calculated metrics from the backtested portfolio
        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("        BACKTEST:  RESULTS");
        MetricsLog.LogInformation("##################################################");
        MetricsLog.LogInformation("    Initial Cash:  ${InitCash:N}", startVal);
        MetricsLog.LogInformation("    Ending Value:  ${FinalVal:N}", endVal);
        MetricsLog.LogInformation("             PnL:  ${PnL:N}", pnl);
        MetricsLog.LogInformation("    Total Return:  {TotalReturn:P2}", totalReturn);
        MetricsLog.LogInformation("    Max Drawdown:  ${MaxDrawdown:N}", maxDrawdown);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("    Total Trades:  {NumTrades:N0}", allTrades.Count);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("        Win Rate:  {WinRate:P2}", winRate);
        MetricsLog.LogInformation("  Winning Trades:  {WinTrades:N0}", wins.Count);
        MetricsLog.LogInformation("        Avg. Win:  ${AvgTrade:N}", avgWin);
        MetricsLog.LogInformation("");
        MetricsLog.LogInformation("   Losing Trades:  {LoseTrades:N0}", losses.Count);
        MetricsLog.LogInformation("       Avg. Loss:  ${AvgTrade:N}", avgLoss);
        MetricsLog.LogInformation(" Max Loss Streak:  {MaxConLoss:N0}", maxLossStreak);
        MetricsLog.LogInformation("");
    }

    /// <summary>
    ///   Calculates the maximum consecutive loss streak and drawdown for the provided list of trades.
    /// </summary>
    /// <param name="trades">The list of the portfolio's trades</param>
    /// <returns>An integer and decimal tuple with the max loss streak and max drawdown</returns>
    private static (int maxLossStreak, decimal maxDrawdown) CalculateMaxLossStreakAndDrawdown(List<Trade> trades)
    {
        // set the max and current streaks/ values to zero before starting
        int maxLossStreak = 0, currentLossStreak = 0;
        decimal currentEquity = 0m, maxEquity = 0m, maxDrawdown = 0m;

        foreach (var trade in trades)
        {
            // if the trade was a loss, update the current (and max if needed) streak, else restart it
            if (!trade.Win)
            {
                if (++currentLossStreak > maxLossStreak) maxLossStreak = currentLossStreak;
            }
            else
            {
                currentLossStreak = 0;
            }

            // update the current (and max if needed) equity
            currentEquity += trade.TotalProfit;
            if (currentEquity > maxEquity) maxEquity = currentEquity;

            // calculate the current (and update max if needed) drawdown
            var drawdown = maxEquity - currentEquity;
            if (drawdown > maxDrawdown) maxDrawdown = drawdown;
        }

        return (maxLossStreak, maxDrawdown);
    }
}