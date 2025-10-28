namespace Common;

/// <summary>
///   This class contains the date-time string format used by the Alpaca API, and two methods. The first method takes
///   in a DateTime object and returns it as a string in the API format, the other takes in a string in the API format
///   and returns it as a DateTime object.
///   <code>string format = "yyyy-MM-ddTHH:mm:ssZ";</code>
/// </summary>
public static class DateFormats
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
        DateTime.ParseExact(date, ApiDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
}