using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions;

/// <summary>
/// Defines a logging interface for structured, extensible message logging with typed support.
/// </summary>
/// <typeparam name="T">
/// The type whose name is used for log scoping.
/// Typically, this represents the class or component being logged.
/// </typeparam>
public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
{
    /// <summary>
    /// Logs a message with the specified log level, details, and optional exception or metadata.
    /// </summary>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="message">The message to be logged.</param>
    /// <param name="exception">An optional exception associated with the log entry.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Log(LogLevel logLevel, string message, Exception? exception = null, object? metadata = null);

    /// <summary>
    /// Logs a trace message with the specified details.
    /// </summary>
    /// <param name="message">The trace-level message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Trace(string message, object? metadata = null) => Log(LogLevel.Trace, message, null, metadata);

    /// <summary>
    /// Logs a debug message with the specified details.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the debug log entry.</param>
    void Debug(string message, object? metadata = null) => Log(LogLevel.Debug, message, null, metadata);

    /// <summary>
    /// Logs an informational message with the specified details.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    /// <param name="metadata">Optional metadata providing additional context for the log entry.</param>
    void Information(string message, object? metadata = null) => Log(LogLevel.Information, message, null, metadata);

    /// <summary>
    /// Logs a warning message with the specified details.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    /// <param name="exception">An optional exception associated with the warning.</param>
    /// <param name="metadata">Optional metadata providing additional context for the warning log.</param>
    void Warning(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Warning, message, exception, metadata);

    /// <summary>
    /// Logs an error message with the specified details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">An optional exception associated with the error.</param>
    /// <param name="metadata">Optional metadata providing additional context for the error log.</param>
    void Error(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Error, message, exception, metadata);

    /// <summary>
    /// Logs a critical error message, typically to indicate a fatal issue or an error that results in the application being unable to continue execution.
    /// </summary>
    /// <param name="message">The critical error message to log.</param>
    /// <param name="exception">An optional exception associated with the error. Can be null if no exception is involved.</param>
    /// <param name="metadata">Optional metadata to include with the log entry. Can be null if no additional data is needed.</param>
    void Fatal(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Critical, message, exception, metadata);
}