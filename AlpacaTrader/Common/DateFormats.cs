namespace Common;

/// <summary>
///   This class holds common date formats.
/// </summary>
public static class DateFormats
{
    /// <summary>
    ///   The format used by Alpaca API URLs.
    /// </summary>
    public const string LongDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
    
    /// <summary>
    ///   Takes in a DataTime object and returns it as a string formatted to work with Alpaca API URLs.
    /// </summary>
    /// <param name="dateTime">The DateTime object to format</param>
    /// <returns>The DateTime formatted to work in API URLs</returns>
    public static string Url(DateTime dateTime) => dateTime.ToString(LongDateTimeFormat);
}