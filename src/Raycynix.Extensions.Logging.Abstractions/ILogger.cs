using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Raycynix.Extensions.Logging.Abstractions;

/// <summary>
/// Defines a typed logger with convenience methods for common log levels.
/// </summary>
/// <typeparam name="T">The type associated with the logger category.</typeparam>
public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
{
    /// <summary>
    /// Writes a log entry using a structured message template and optional template arguments.
    /// </summary>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="exception">An optional exception associated with the log entry.</param>
    /// <param name="message">The structured message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Log(LogLevel logLevel, Exception? exception, string message, params object?[]? args);

    #region Trace

    /// <summary>
    /// Writes a trace-level log entry without template arguments.
    /// </summary>
    /// <param name="message">The trace message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Trace(string message) =>
        Log(LogLevel.Trace, null, message, null);

    /// <summary>
    /// Writes a trace-level log entry.
    /// </summary>
    /// <param name="message">The trace message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Trace(string message, params object?[]? args) =>
        Log(LogLevel.Trace, null, message, args);

    /// <summary>
    /// Writes a trace-level log entry with an associated exception.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The trace message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Trace(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Trace, exception, message, args);

    #endregion

    #region Debug

    /// <summary>
    /// Writes a debug-level log entry without template arguments.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Debug(string message) =>
        Log(LogLevel.Debug, null, message, null);

    /// <summary>
    /// Writes a debug-level log entry.
    /// </summary>
    /// <param name="message">The debug message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Debug(string message, params object?[]? args) =>
        Log(LogLevel.Debug, null, message, args);

    /// <summary>
    /// Writes a debug-level log entry with an associated exception.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The debug message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Debug(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Debug, exception, message, args);

    #endregion

    #region Information

    /// <summary>
    /// Writes an informational log entry without template arguments.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Information(string message) =>
        Log(LogLevel.Information, null, message, null);

    /// <summary>
    /// Writes an informational log entry.
    /// </summary>
    /// <param name="message">The informational message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Information(string message, params object?[]? args) =>
        Log(LogLevel.Information, null, message, args);

    /// <summary>
    /// Writes an informational log entry with an associated exception.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The informational message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Information(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Information, exception, message, args);

    #endregion

    #region Warning

    /// <summary>
    /// Writes a warning log entry without template arguments.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Warning(string message) =>
        Log(LogLevel.Warning, null, message, null);

    /// <summary>
    /// Writes a warning log entry.
    /// </summary>
    /// <param name="message">The warning message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Warning(string message, params object?[]? args) =>
        Log(LogLevel.Warning, null, message, args);

    /// <summary>
    /// Writes a warning log entry.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The warning message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Warning(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Warning, exception, message, args);

    #endregion

    #region Error

    /// <summary>
    /// Writes an error log entry without template arguments.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Error(string message) =>
        Log(LogLevel.Error, null, message, null);

    /// <summary>
    /// Writes an error log entry.
    /// </summary>
    /// <param name="message">The error message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Error(string message, params object?[]? args) =>
        Log(LogLevel.Error, null, message, args);

    /// <summary>
    /// Writes an error log entry.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The error message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Error(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Error, exception, message, args);

    #endregion

    #region Fatal

    /// <summary>
    /// Writes a critical log entry without template arguments.
    /// </summary>
    /// <param name="message">The critical message to log.</param>
    [MessageTemplateFormatMethod("message")]
    void Fatal(string message) =>
        Log(LogLevel.Critical, null, message, null);

    /// <summary>
    /// Writes a critical log entry.
    /// </summary>
    /// <param name="message">The critical message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Fatal(string message, params object?[]? args) =>
        Log(LogLevel.Critical, null, message, args);

    /// <summary>
    /// Writes a critical log entry.
    /// </summary>
    /// <param name="exception">The exception associated with the log entry.</param>
    /// <param name="message">The critical message template to log.</param>
    /// <param name="args">Optional arguments used to populate the message template.</param>
    [MessageTemplateFormatMethod("message")]
    void Fatal(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Critical, exception, message, args);

    #endregion
}
