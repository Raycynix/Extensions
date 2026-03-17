using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions;

/// <summary>
/// Defines a typed logger with convenience methods for common log levels.
/// </summary>
/// <typeparam name="T">The type associated with the logger category.</typeparam>
public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
{
    /// <summary>
    /// Writes a log entry with optional exception and metadata.
    /// </summary>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="message">The message to be logged.</param>
    /// <param name="exception">An optional exception associated with the log entry.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Log(LogLevel logLevel, string message, Exception? exception = null, object? metadata = null);

    /// <summary>
    /// Writes a trace-level log entry.
    /// </summary>
    /// <param name="message">The trace-level message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Trace(string message, object? metadata = null) => Log(LogLevel.Trace, message, null, metadata);

    /// <summary>
    /// Writes a debug-level log entry.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the debug log entry.</param>
    void Debug(string message, object? metadata = null) => Log(LogLevel.Debug, message, null, metadata);

    /// <summary>
    /// Writes an informational log entry.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Information(string message, object? metadata = null) => Log(LogLevel.Information, message, null, metadata);

    /// <summary>
    /// Writes a warning log entry.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    /// <param name="exception">An optional exception associated with the warning.</param>
    /// <param name="metadata">Optional metadata providing additional context for the warning log.</param>
    void Warning(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Warning, message, exception, metadata);

    /// <summary>
    /// Writes an error log entry.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">An optional exception associated with the error.</param>
    /// <param name="metadata">Optional metadata providing additional context for the error log.</param>
    void Error(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Error, message, exception, metadata);

    /// <summary>
    /// Writes a critical log entry.
    /// </summary>
    /// <param name="message">The critical error message to log.</param>
    /// <param name="exception">An optional exception associated with the error. Can be null if no exception is involved.</param>
    /// <param name="metadata">Optional metadata to include with the log entry. Can be null if no additional data is needed.</param>
    void Fatal(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Critical, message, exception, metadata);
}
