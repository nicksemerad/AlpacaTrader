namespace StrategyTests;

using Component;
using Strategy;

/// <summary>
///   This class contains tests for the Strategy class methods.
/// </summary>
[TestClass]
public class Tests
{
    /// <summary>
    ///   A list of decimals that acts as the close prices for 16 bars that will be used for testing.
    /// </summary>
    private static readonly List<decimal> ClosePrices =
    [
        7104m, 7058.4m, 7269.1m, 7398.3m, 7489.45m, 7505.45m, 7496.25m, 7579.55m,
        7540.1m, 7569.45m, 7598.35m, 7545.8m, 7587.35m, 7576.8m, 7658.95m, 7763.7m
    ];

    /// <summary>
    ///   A list of 7 expected values that should be the result of SMA and
    ///   EMA function calls when using the _closePrices and _period.
    /// </summary>
    private static readonly List<decimal> ExpectedValues =
        [7401.005m, 7436.886m, 7456.688m, 7480.445m, 7497.964m, 7527.234m, 7570.228m];

    /// <summary>
    ///   The period value that the _closePrices and _expectedValues use.
    /// </summary>
    private const int Period = 10;

    /// <summary>
    ///   A list of bars that will be used for testing, each containing a close price from _closePrices
    /// </summary>
    private static readonly List<Bar> Bars = GetBars();

    /// <summary>
    ///   Creates a list of Bars by using each ClosePrices decimal to make a Bar. The timestamps
    ///   are added only to simulate real Bars that are sorted in order of their time. The timestamps start at 16
    ///   minutes ago and get 1 minute closer for each bar with the 16th bar being 1 minute ago.
    /// </summary>
    private static List<Bar> GetBars()
    {
        DateTime now = DateTime.Now;
        List<Bar> bars = new List<Bar>();
        for (int i = 0; i < 16; i++)
            bars.Add(
                new Bar
                {
                    Timestamp = now.AddMinutes(16 - i),
                    Close = ClosePrices[i]
                }
            );
        return bars;
    }

    /// <summary>
    ///   Test that calling GetSma on the first 10 bars results in the
    ///   expected value, which is the first element in _expectedValues.
    /// </summary>
    [TestMethod]
    public void TestGetSma_ReturnsExpectedValue()
    {
        // InitBars();
        decimal sma = Strategy.GetSma(Bars.Take(Period).ToList(), Period);
        decimal expectedValue = ExpectedValues[0];
        Assert.AreEqual(Math.Round(sma, 3), expectedValue);
    }

    /// <summary>
    ///   Test that calling GetEma using _bars returns the expected values. This is done by first passing the
    ///   function 10 bars. Because the period is also 10, the 10th element should use the SMA value, which is the
    ///   first element in _expectedValues. From there, each EMA value incorporates the previous one, so 11 bars are
    ///   passed, then 12, etc until all 16 bars are passed. At each stage the expected EMA value is checked to be
    ///   the same as the corresponding value in _expectedValues.
    /// </summary>
    [TestMethod]
    public void TestGetEma_ReturnsExpectedValue()
    {
        // InitBars();
        for (int i = Period; i <= 16; i++)
        {
            decimal ema = Strategy.GetEma(Bars.Take(i).ToList(), Period);
            decimal expectedValue = ExpectedValues[i - Period];
            Assert.AreEqual(Math.Round(ema, 3), expectedValue);
        }
    }
    
    /// <summary>
    ///   Test that calling GetSma with a period of zero throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetSma_PeriodOfZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetSma(Bars, 0));
    }
        
    /// <summary>
    ///   Test that calling GetSma with an empty list of bars throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetSma_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetSma([], Period));
    }
        
    /// <summary>
    ///   Test that calling GetSma with a list smaller than the period value throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetSma_ListCountLessThanPeriod_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetSma(Bars, 100));
    }
    
    /// <summary>
    ///   Test that calling GetEma with a period of zero throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetEma_PeriodOfZero_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetEma(Bars, 0));
    }
        
    /// <summary>
    ///   Test that calling GetSma with an empty list of bars throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetEma_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetEma([], Period));
    }
        
    /// <summary>
    ///   Test that calling GetEma with a list smaller than the period value throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void TestGetEma_ListCountLessThanPeriod_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Strategy.GetEma(Bars, 100));
    }
}