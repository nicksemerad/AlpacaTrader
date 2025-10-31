using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
///   This class sets up a Logger from the Microsoft Extensions Logging package, and builds it with the custom
///   configuration. Right now that configuration is just console logging and a minimum level of "Information", but in
///   the future I plan to include logging to files so they can be kept for longer periods of time.
/// </summary>
public static class Logger
{
    /// <summary>
    ///   A private ILoggerFactory that is created with the custom configuration (AddConsole adds console logging,
    ///   SetMinimumLevel sets the lowest level of event that should still be logged).
    /// </summary>
    private static readonly ILoggerFactory Factory =
        LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(
                options =>
                {
                    options.SingleLine = true; // puts all the output on one line
                    options.TimestampFormat = "[HH:mm:ss] "; // adds a timestamp (space is on purpose)
                }
            ).SetMinimumLevel(LogLevel.Information);
        });
    
    /// <summary>
    ///   Creates a new Logger in the category of whatever is passed as a parameter, typically the name of the class
    ///   where the logger is being used. This is done by using the nameof() function to get the class name. An example
    ///   of this for the Client class is shown below.
    ///   <code>
    ///     private static ILogger _logger = Logger.Create(nameof(Client));
    ///   </code>
    /// </summary>
    /// <param name="categoryName">The name of the logger category, determined by nameof(class to log)</param>
    /// <returns>A new categoryName ILogger</returns>
    public static ILogger Create(string categoryName) => Factory.CreateLogger(categoryName);
}