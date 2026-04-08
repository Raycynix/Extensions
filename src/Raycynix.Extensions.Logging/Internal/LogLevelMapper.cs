using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Raycynix.Extensions.Logging.Internal;

/// <summary>
/// Maps Microsoft logging levels to Serilog event levels.
/// </summary>
internal static class LogLevelMapper
{
    /// <summary>
    /// Converts a Microsoft log level to the equivalent Serilog event level.
    /// </summary>
    /// <param name="level">The Microsoft log level to convert.</param>
    /// <returns>The matching Serilog event level.</returns>
    public static LogEventLevel ToSerilog(LogLevel level) => level switch
    {
        LogLevel.Trace => LogEventLevel.Verbose,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Critical => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };
}
