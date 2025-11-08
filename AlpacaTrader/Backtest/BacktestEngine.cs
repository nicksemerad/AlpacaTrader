namespace Backtest;

using Common;
using Strategy;
using Component;
using Database;
using Microsoft.Extensions.Logging;

/// <summary>
///   This class tests different strategies by simulating historical bars and the passage of time to test how they may
///   have performed.
/// </summary>
public class BacktestEngine
{
    public static async Task Main(string[] _)
    {
        // get the bars
        var bars = await DbOperations.GetBarsBySymbolTimeframeAsync(
            "AAPL", "1T", new DateTime(2024, 1, 1), new DateTime(2024, 6, 1));

        // make the strategy with the first 250 bars (warmup period)
        var strategy = new RsiStrategy(bars[..250]);

        // make and run the engine
        var engine = new BacktestEngine(strategy, bars);
        engine.Run();
    }

    /// <summary>
    ///   The logger object for this BacktestEngine.
    /// </summary>
    private static readonly ILogger EngineLog = Logger.Create(nameof(BacktestEngine));

    /// <summary>
    ///   The strategy to backtest with.
    /// </summary>
    private readonly Strategy _strategy;

    /// <summary>
    ///   All the bars that will be backtested with.
    /// </summary>
    private readonly List<Bar> _bars;

    /// <summary>
    ///   The paper portfolio to keep track of cash, positions, orders, etc. 
    /// </summary>
    private readonly PaperPortfolio _portfolio;

    /// <summary>
    ///   Constructs a new BacktestEngine for the passed parameters. Right now it only takes in 1 symbol's bars, but
    ///   this will be updated in the future. I decided against having the symbol as a parameter here in the
    ///   constructor, which is likely to change. The reason I did this is multithreading. If it's running a strategy
    ///   and backtesting it on many symbols, it makes sense to me that I make each of them on their own thread. We
    ///   will see, though!
    /// </summary>
    /// <param name="strategy">The strategy to backtest</param>
    /// <param name="bars">The historical bars to use for the backtest</param>
    /// <param name="initialCash">The portfolio's starting cash, defaults to 100k</param>
    private BacktestEngine(Strategy strategy, List<Bar> bars, decimal initialCash = 100_000m)
    {
        _strategy = strategy;
        _bars = bars;
        _portfolio = new PaperPortfolio(initialCash);
    }

    /// <summary>
    ///   Logs the initial conditions, runs the backtest, and logs the results. Before the test begins running, the
    ///   strategy will have already been constructed with some bars to start with. Usually 250 as that is enough to
    ///   smooth the indicator values from the skender stock library. These initial 250 bars are just the first 250
    ///   bars in the total bars. When the test begins, a for loop is used to iterate through all the total bars,
    ///   starting at the first bar not included in the strategy's initial bars.
    ///   <example>
    ///     For example, if the strategy is initialized with 250 bars (sBars), it has indices 0-249. Assume there are
    ///     1,000 total bars (tBars), so indices 0-999. They overlap on all sBar indices, as that is how the strategy
    ///     is initialized. The loop will start at 250 and iterate to 999, adding the tBar at the index to the end of
    ///     sBars. Each time a new bar is added to the strategy, a signal is generated and handled. This (very roughly)
    ///     simulates the passing of time.
    ///   </example>
    /// </summary>
    private void Run()
    {
        LogSetup();

        // iterate from strategy bars count to total bar count
        for (int i = _strategy.Bars.Count; i < _bars.Count; i++)
        {
            var newBar = _bars[i];
            _strategy.Update(newBar);

            // get and handle the new bar's signal and record the portfolio value if
            // the signal handler returned true indicating that an order was made
            if (HandleSignal(_strategy.GetSignal(), newBar))
                _portfolio.RecordValue([newBar]);
        }

        LogResults();
    }

    /// <summary>
    ///   Handles a signal produced by the strategy because of the newBar. When a buy or sell order is signaled, as of
    ///   now, we try and buy as many shares as we can afford or sell every share we own. If the order succeeds, this
    ///   method returns true. If it fails, or a hold was signaled, false is returned. True means that the portfolio
    ///   has changed; false means it has not. This is so that the portfolio value is only calculated and stored if
    ///   the new bar changed it. This method will be changed as time goes on, this is just a first draft. Here are a
    ///   few things that will likely be changed:
    ///   <list type="bullet">
    ///     <item>When a sell order is signaled, we sell all of our shares</item>
    ///     <item>Similarly, when a buy order is signaled, we buy as many shares as our Cash allows</item>
    ///     <item>Shares are only bought and sold in whole numbers, no fractional shares</item>
    ///   </list>
    /// </summary>
    /// <param name="signal">The TradeSignal sent by the strategy</param>
    /// <param name="newBar">The new bar which caused the signal</param>
    private bool HandleSignal(TradeSignal signal, Bar newBar)
    {
        var (symbol, price, date) = (newBar.Symbol, newBar.Close, newBar.Date);
        var numShares = _portfolio.GetSymbolShares(symbol);

        return signal switch
        {
            // on a sell signal, try to sell every share (if we have any)
            TradeSignal.Sell => numShares > 0 &&  _portfolio.TrySell(symbol, numShares, price, date),
            // on a buy signal, first calculate how many shares we can buy with our cash, then try to buy them
            TradeSignal.Buy => numShares == 0 && (int)(_portfolio.Cash / price) is var sharesToBuy and > 0 &&
                               _portfolio.TryBuy(symbol, sharesToBuy, price, date),
            // when the signal was to Hold
            _ => false
        };
    }

    /// <summary>
    ///   Logs the initial state of the backtest.
    /// </summary>
    private void LogSetup()
    {
        var dateRange = $"{_bars.First().Date:yyyy-MM-dd} -> {_bars.Last().Date:yyyy-MM-dd}";

        EngineLog.LogInformation("##################################################");
        EngineLog.LogInformation("        BACKTEST:  START");
        EngineLog.LogInformation("##################################################");
        EngineLog.LogInformation("          Symbol:  {Symbol}", _bars[0].Symbol);
        EngineLog.LogInformation("    Initial cash:  {Cash:C}", _portfolio.InitialCash);
        EngineLog.LogInformation("      Total bars:  {Bars:N0}", _bars.Count);
        EngineLog.LogInformation("     Time period:  {Period}", dateRange);
    }

    /// <summary>
    ///   Logs the ending state of the backtest, along with some simple metrics.
    /// </summary>
    private void LogResults()
    {
        var initCash = _portfolio.InitialCash;
        var finalValue = _portfolio.ValueHistory.Last().value;
        var netGain = finalValue - initCash;
        var totalReturn = netGain / initCash;
        var numTrades = _portfolio.OrderHistory.Count;
    
        EngineLog.LogInformation("##################################################");
        EngineLog.LogInformation("        BACKTEST:  RESULTS");
        EngineLog.LogInformation("##################################################");
        EngineLog.LogInformation("    Initial Cash:  ${InitCash:N2}", initCash);
        EngineLog.LogInformation("    Ending Value:  ${Final:N2}", finalValue);
        EngineLog.LogInformation("       Net Gains:  ${NetGain:N2}", netGain);
        EngineLog.LogInformation("         Returns:  {TotalReturn:P2}", totalReturn);
        EngineLog.LogInformation("    Total Trades:  {NumTrades:N0}", numTrades);
        EngineLog.LogInformation(" Avg. Trade Gain:  ${TradeGain:N2}", netGain / numTrades);
    }
}