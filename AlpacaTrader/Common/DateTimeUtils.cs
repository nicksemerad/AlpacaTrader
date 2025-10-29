namespace Common;

using System.Globalization;

/// <summary>
///   This class contains the date-time string format used by the Alpaca API, and two methods. The first method takes
///   in a DateTime object and returns it as a string in the API format, the other takes in a string in the API format
///   and returns it as a DateTime object.
///   <code>string format = "yyyy-MM-ddTHH:mm:ssZ";</code>
/// </summary>
public static class DateTimeUtils
{
    /// <summary>
    ///   The format used by Alpaca API URLs and endpoint responses.
    /// </summary>
    private const string ApiDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    /// <summary>
    ///   Takes in a DataTime object and returns it as a string formatted to work with Alpaca API URLs.
    /// </summary>
    /// <param name="dateTime">The DateTime object to format</param>
    /// <returns>The passed date-time parsed into a string in the format: "yyyy-MM-ddTHH:mm:ssZ"</returns>
    public static string ToUrlString(DateTime dateTime) => dateTime.ToString(ApiDateTimeFormat);

    /// <summary>
    ///   Takes in a date-time string in the Alpaca API URLs and response objects format and parses it into a DateTime.
    /// </summary>
    /// <param name="date">The date string in Alpaca API format: "yyyy-MM-ddTHH:mm:ssZ"(</param>
    /// <returns>The string's date time parsed into a C# DateTime object</returns>
    public static DateTime ToDateTime(string date) =>
        DateTime.ParseExact(date, ApiDateTimeFormat, new CultureInfo("en-US"));

    /// <summary>
    ///   Converts a DateTime object that is in Eastern Standard Time (EST) to be in UTC instead.
    /// </summary>
    /// <param name="dateTime">The DateTime object in EST that needs to be converted</param>
    /// <returns>A new DateTime object that has the same time as the parameter, but in UTC</returns>
    public static DateTime ConvertEstToUtc(DateTime dateTime)
    {
        try
        {
            var estZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, estZoneInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType()} occurred while converting DateTime in EST to UTC: {ex.Message}");
            throw;
        }
    }
}