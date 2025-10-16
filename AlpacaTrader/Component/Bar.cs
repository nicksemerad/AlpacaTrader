namespace Component;

using System.Text.Json.Serialization;

/// <summary>
///   Bar object, for use with the Alpaca API that returns JSON objects representing bars.
/// </summary>
public class Bar
{
    public string Symbol { get; set; }
    
    [JsonPropertyName("t")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("o")]
    public decimal Open { get; set; }
    
    [JsonPropertyName("h")]
    public decimal High { get; set; }
    
    [JsonPropertyName("l")]
    public decimal Low { get; set; }
    
    [JsonPropertyName("c")]
    public decimal Close { get; set; }
    
    [JsonPropertyName("v")]
    public long Volume { get; set; }
    
    [JsonPropertyName("n")]
    public int TradeCount { get; set; }
    
    [JsonPropertyName("vw")]
    public decimal VolumeWeightedAverage { get; set; }

    public Bar()
    {
        
    }
}