using Component;
using Skender.Stock.Indicators;

namespace Indicators;

/// <summary>
///   Simple Moving Average indicator manager.
/// </summary>
public class SmaIndicator
{
    private readonly List<Bar> _bars;
    private List<SmaResult>? _series;
}

/// <summary>
///   Exponential Moving Average indicator manager.
/// </summary>
public class EmaIndicator
{
    private readonly List<Bar> _bars;
    private List<EmaResult>? _series;
}

/// <summary>
///   Moving Average Convergence Divergence indicator manager.
/// </summary>
public class MacdIndicator
{
    private readonly List<Bar> _bars;
    private List<MacdResult>? _series;
}

/// <summary>
///   Relative Strength Index indicator manager.
/// </summary>
public class RsiIndicator
{
    private readonly List<Bar> _bars;
    private List<RsiResult>? _series;
}

/// <summary>
///   Bollinger Bands indicator manager.
/// </summary>
public class BollingerBandsIndicator
{
    private readonly List<Bar> _bars;
    private List<BollingerBandsResult>? _series;
}
