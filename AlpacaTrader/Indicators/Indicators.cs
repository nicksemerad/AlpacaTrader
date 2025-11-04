namespace Indicators;

using Common;
using Component;
using Skender.Stock.Indicators;
using Microsoft.Extensions.Logging;

/// <summary>
///   This method handles the process of converting a list of bars to a list of indicator values, as well as verifying
///   that there are enough bars to calculate the indicator, and enough to calculate it accurately.
/// </summary>
public static class Indicators
{
    /// <summary>
    ///   The ILogger used to log events in this class.
    /// </summary>
    private static readonly ILogger IndicatorLogger = Logger.Create(nameof(Indicators));

    /// <summary>
    ///   Gets a series with the simple moving average over the specified period for the bars passed bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the SMA with</param>
    /// <param name="period">The number of past bars to calculate the SMA with</param>
    /// <returns>A list of SMA results for the provided bars and period</returns>
    /// <exception cref="InvalidQuotesException">If there aren't enough bars to calculate the SMA</exception>
    public static List<SmaResult> GetSmaSeries(List<Bar> bars, int period)
    {
        if (bars.Count < period)
            throw new InvalidQuotesException("SMA requires at least (period + 1) bars");

        var relevantBars = bars.TakeLast(period + 1);
        return relevantBars.GetSma(period).ToList();
    }

    /// <summary>
    ///   Gets a series with the exponential moving average over the specified period for the specified bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the EMA with</param>
    /// <param name="period">The number of past bars to calculate the EMA with</param>
    /// <returns>A list of EMA results for the provided bars and period</returns>
    /// <exception cref="InvalidQuotesException">If there aren't enough bars to calculate the EMA</exception>
    public static List<EmaResult> GetEmaSeries(List<Bar> bars, int period)
    {
        // EMA requires 2*N or N+100 bars for accurate results for period N
        var min = period >= 200 ? 2 * period : period + 100;
        if (bars.Count < min)
            throw new InvalidQuotesException(
                "EMA requires at least (2*period or period+100) bars, whichever is greater");
        if (bars.Count < period + 250)
            IndicatorLogger.LogWarning("EMA requires (period+250) bars for accurate results");

        var relevantBars = bars.TakeLast(period + 251);
        return relevantBars.GetEma(period).ToList();
    }

    /// <summary>
    ///   Gets a series with the moving average convergence divergence over the specified fast, slow, and signal
    ///   periods for the provided bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the MACD with</param>
    /// <param name="fastPeriod">The shorter term EMA to calculate the MACD with</param>
    /// <param name="slowPeriod">The longer term EMA to calculate the MACD with</param>
    /// <param name="signalPeriod">The period to calculate the signal for</param>
    /// <returns>A list of MACD results for the provided bars and period</returns>
    /// <exception cref="InvalidQuotesException">If there aren't enough bars to calculate the MACD</exception>
    public static List<MacdResult> GetMacdSeries(
        List<Bar> bars, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        var period = slowPeriod + signalPeriod;
        var min = period >= 200 ? 2 * period : period + 100;
        if (bars.Count < min)
            throw new InvalidQuotesException(
                "MACD requires at least (2×(Slow+Signal) or Slow+Signal+100) bars, whichever is greater");
        if (bars.Count < period + 250)
            IndicatorLogger.LogWarning("MACD requires (Slow+Signal+250) bars for accurate results");

        var relevantBars = bars.TakeLast(period + 251);
        return relevantBars.GetMacd(fastPeriod, slowPeriod, signalPeriod).ToList();
    }

    /// <summary>
    ///   Gets a series with the relative strength index over the specified period for the provided bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the RSI with</param>
    /// <param name="period">The number of past bars to calculate the RSI with</param>
    /// <returns>A list of RSI results for the provided bars and period</returns>
    /// <exception cref="InvalidQuotesException">If there aren't enough bars to calculate the RSI</exception>
    public static List<RsiResult> GetRsiSeries(List<Bar> bars, int period = 14)
    {
        if (bars.Count < period + 100)
            throw new InvalidQuotesException("RSI requires at least (period+100) bars");
        if (bars.Count < 10 * period)
            IndicatorLogger.LogWarning("RSI requires at least (10*period) bars for accurate results");

        var relevantBars = bars.TakeLast(10 * period + 1);
        return relevantBars.GetRsi(period).ToList();
    }

    /// <summary>
    ///   Gets a series with the bollinger bands over the specified period and standard dev. for the provided bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the bands with</param>
    /// <param name="period">The number of past bars to calculate the bands with</param>
    /// <param name="standardDeviations">The number of standard deviations above and below the bands are</param>
    /// <returns>A list of bollinger bands results for the provided bars and period</returns>
    /// <exception cref="InvalidQuotesException">If there aren't enough bars to calculate the bands</exception>
    public static List<BollingerBandsResult> GetBollingerBands(
        List<Bar> bars, int period = 20, double standardDeviations = 2)
    {
        if (period < 2)
            throw new InvalidQuotesException("Bollinger bands requires a lookback period of at least 2");
        if (standardDeviations < 1)
            throw new InvalidQuotesException("Bollinger bands requires a standard deviation of at least 1");
        if (bars.Count < period)
            throw new InvalidQuotesException("Bollinger Bands requires at least (period) bars");

        var relevantBars = bars.TakeLast(period + 1);
        return relevantBars.GetBollingerBands(period, standardDeviations).ToList();
    }

    public static void Main(string[] args)
    {
        List<int> l = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        Console.WriteLine(string.Join(", ", l.TakeLast(50)));
    }
}

/// <summary>
///   A class of extensions that make the indicators easier to work with.
/// </summary>
public static class IndicatorsExtensions
{
    /// <summary>
    ///   Converts a nullable double to a decimal.
    /// </summary>
    /// <param name="value">The double to convert</param>
    /// <returns>The double value in decimal form</returns>
    public static decimal? ToDecimal(this double? value)
    {
        return Convert.ToDecimal(value);
    }

    /// <summary>
    ///   Converts a tuple of three nullable doubles to a tuple of three nullable decimals.
    /// </summary>
    /// <param name="values">The tuple to convert</param>
    /// <returns>The decimal tuple</returns>
    public static (decimal?, decimal?, decimal?) ToDecimals(this (double? a, double? b, double? c) values)
    {
        return (values.a.ToDecimal(), values.b.ToDecimal(), values.c.ToDecimal());
    }

    /// <summary>
    ///   Gets the underlying value of an indicator series at a specific index as a decimal.
    /// </summary>
    /// <param name="series">The series of indicator results being extended with this faux indexer</param>
    /// <param name="index">The index of the underlying value to get and convert to a decimal</param>
    /// <typeparam name="T">The type of result series to get the value from</typeparam>
    /// <returns>The value of the underlying indicator at the specified index as a decimal</returns>
    /// <exception cref="InvalidOperationException">If the type of the result series is unsupported</exception>
    public static decimal? At<T>(this List<T> series, Index index)
    {
        var current = series[index] switch
        {
            SmaResult sma => sma.Sma,
            EmaResult ema => ema.Ema,
            RsiResult rsi => rsi.Rsi,
            _ => throw new InvalidOperationException("Unsupported indicator type.")
        };
        return current.ToDecimal();
    }

    /// <summary>
    ///   Gets a tuple of the underlying values of a Macd series at a specific index as a tuple of decimals.
    /// </summary>
    /// <param name="series">The series of Macd results being extended with this faux indexer</param>
    /// <param name="index">The index of the underlying values to get and convert to a decimal tuple</param>
    /// <returns>The value of the underlying MacdResult at the specified index as a decimal</returns>
    public static (decimal? macd, decimal? signal, decimal? histogram) At(this List<MacdResult> series, Index index)
    {
        var current = series[index];
        return (current.Macd, current.Signal, current.Histogram).ToDecimals();
    }

    /// <summary>
    ///   Gets a tuple of the underlying values of a Bollinger Bands series at a specific index as a tuple of decimals.
    /// </summary>
    /// <param name="series">The series of Bollinger Bands results being extended with this faux indexer</param>
    /// <param name="index">The index of the underlying values to get and convert to a decimal tuple</param>
    /// <returns>The value of the underlying BollingerBandsResult at the specified index as a decimal</returns>
    public static (decimal? upper, decimal? mid, decimal? lower) At(this List<BollingerBandsResult> series, Index index)
    {
        var current = series[index];
        return (current.UpperBand, current.Sma, current.LowerBand).ToDecimals();
    }
}