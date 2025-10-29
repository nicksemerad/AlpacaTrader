namespace Component;

using Common;
using Newtonsoft.Json;

/// <summary>
///   This class represents a single trading calendar day as is returned from the Alpaca API calendar endpoint. To
///   make explaining the class properties and methods easier, I will refer to an instance of this class as "today"
///   i.e. the Date property is "today's date".
/// </summary>
public class CalendarDay
{
    /// <summary>
    ///   Today's date.
    /// </summary>
    [JsonProperty("date")]
    public DateOnly Date { get; set; }

    /// <summary>
    ///   The time that the market opens today, usually 09:30AM eastern.
    /// </summary>
    [JsonProperty("open")]
    public TimeOnly OpenTime { get; set; }

    /// <summary>
    ///   The time that the market closes today, usually 16:00PM eastern.
    /// </summary>
    [JsonProperty("close")]
    public TimeOnly CloseTime { get; set; }

    /// <summary>
    ///   The time that the pre-market session opens today, usually 04:00AM eastern. This uses a custom
    ///   JsonConverter when reading the calendar API response because there are errors in this field.
    /// </summary>
    [JsonProperty("session_open")]
    [JsonConverter(typeof(CalendarApiTimeOnlyConverter))]
    public TimeOnly SessionOpenTime { get; set; }

    /// <summary>
    ///   The time that the after-market session closes today, usually 20:00PM eastern. This uses a custom
    ///   JsonConverter when reading the calendar API response because there are errors in this field.
    /// </summary>
    [JsonProperty("session_close")]
    [JsonConverter(typeof(CalendarApiTimeOnlyConverter))]
    public TimeOnly SessionCloseTime { get; set; }

    /// <summary>
    ///   Gets today's market open datetime in UTC.
    /// </summary>
    public DateTime GetMarketOpenUtc() => DateTimeUtils.ConvertEstToUtc(new DateTime(Date, OpenTime));

    /// <summary>
    ///   Gets today's market close datetime in UTC.
    /// </summary>
    public DateTime GetMarketCloseUtc() => DateTimeUtils.ConvertEstToUtc(new DateTime(Date, CloseTime));

    /// <summary>
    ///   Overrides the object ToString so CalendarDays can be printed showing the date, market open time, and market
    ///   close times in EST.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{Date:yyyy-MM-dd}]: {OpenTime:HH:mm} - {CloseTime:HH:mm}";
    }
}