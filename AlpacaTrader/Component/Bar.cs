namespace Component;
using Common;

using Newtonsoft.Json;

/// <summary>
///   This class represents a single Bar in a stock's candlestick chart.
/// </summary>
public class Bar
{
    /// <summary>
    ///   This Bar's stock ticker symbol.
    /// </summary>
    [JsonIgnore]
    public string Symbol { get; set; }
    
    /// <summary>
    ///   The time this Bar's period started. For a 1-hour bar it's the start of the hour.
    /// </summary>
    [JsonProperty("t")]
    public DateTime Timestamp { get; set; }
        
    /// <summary>
    ///   The first price the stock traded at in the time period.
    /// </summary>
    [JsonProperty("o")]
    public decimal Open { get; set; }
        
    /// <summary>
    ///   The highest price the stock traded at in the time period.
    /// </summary>
    [JsonProperty("h")]
    public decimal High { get; set; }
        
    /// <summary>
    ///   The lowest price the stock traded at in the time period.
    /// </summary>
    [JsonProperty("l")]
    public decimal Low { get; set; }
        
    /// <summary>
    ///   The last price the stock traded at in the time period.
    /// </summary>
    [JsonProperty("c")]
    public decimal Close { get; set; }
        
    /// <summary>
    ///   The total number of shares traded in the time period.
    /// </summary>
    [JsonProperty("v")]
    public int Volume { get; set; }
        
    /// <summary>
    ///   The total number of trades made in the time period.
    /// </summary>
    [JsonProperty("n")]
    public int TradeCount { get; set; }
        
    /// <summary>
    ///   The average price of each share traded in the time period. 
    /// </summary>
    [JsonProperty("vw")]
    public decimal VolumeWeightedAverage { get; set; }

    /// <summary>
    ///   Returns this Bar in string format.
    /// </summary>
    /// <returns>The string representation of this Bar</returns>
    public override string ToString()
    {
        return $"{Symbol} [{DateFormats.Url(Timestamp)}] - VWA: ${VolumeWeightedAverage:N2}\n" + 
               $"O: ${Open:N2} - C: ${Close:N2}\n" + $"H: ${High:N2} - L: ${Low:N2}\n";
    }
}