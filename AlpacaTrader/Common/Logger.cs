using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
///   This class sets up a Logger from the Microsoft Extensions Logging package, and builds it with the custom
///   configuration. Right now that configuration is just console logging and a minimum level of "Information", but in
///   the future I plan to include logging to files so they can be kept for longer periods of time.
/// </summary>
public class Logger
{
    /// <summary>
    ///   A private ILoggerFactory that is created with the custom configuration (AddConsole adds console logging,
    ///   SetMinimumLevel sets the lowest level of event that should still be logged).
    /// </summary>
    private static readonly ILoggerFactory Factory = 
        LoggerFactory.Create(config => config.AddConsole().SetMinimumLevel(LogLevel.Information));
    
    /// <summary>
    ///   A generic function that returns a new ILogger for type T that was created using the ILoggerFactory built
    ///   with the custom configuration.
    /// </summary>
    /// <typeparam name="T">The category type of the ILogger</typeparam>
    /// <returns>A new category T ILogger</returns>
    public static ILogger<T> Create<T>() => Factory.CreateLogger<T>();
}