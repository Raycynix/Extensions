using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging.Internal;

namespace Raycynix.Extensions.Logging.Implementations;

/// <summary>
/// Adapts the Raycynix typed logger abstraction to a Serilog-backed implementation
/// and enriches log events with correlation information when it is available.
/// </summary>
/// <typeparam name="T">The type associated with the logger category.</typeparam>
public class Logger<T>(Serilog.ILogger logger) : Abstractions.ILogger<T>
{
    private readonly Serilog.ILogger _logger = logger.ForContext<T>();

    /// <inheritdoc />
    public void Log(LogLevel logLevel, Exception? exception, string messageTemplate, params object?[]? args)
    {
        var level = LogLevelMapper.ToSerilog(logLevel);
        var logger = GetContextualLogger();

        logger.Write(level, exception, messageTemplate, args);
    }

    #region Trace

    /// <inheritdoc />
    public void Trace(string message) =>
        Log(LogLevel.Trace, null, message, null);

    /// <inheritdoc />
    public void Trace(string message, params object?[]? args) =>
        Log(LogLevel.Trace, null, message, args);

    /// <inheritdoc />
    public void Trace(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Trace, exception, message, args);

    #endregion

    #region Debug

    /// <inheritdoc />
    public void Debug(string message) =>
        Log(LogLevel.Debug, null, message, null);

    /// <inheritdoc />
    public void Debug(string message, params object?[]? args) =>
        Log(LogLevel.Debug, null, message, args);

    /// <inheritdoc />
    public void Debug(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Debug, exception, message, args);

    #endregion

    #region Information

    /// <inheritdoc />
    public void Information(string message) =>
        Log(LogLevel.Information, null, message, null);

    /// <inheritdoc />
    public void Information(string message, params object?[]? args) =>
        Log(LogLevel.Information, null, message, args);

    /// <inheritdoc />
    public void Information(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Information, exception, message, args);

    #endregion

    #region Warning

    /// <inheritdoc />
    public void Warning(string message) =>
        Log(LogLevel.Warning, null, message, null);

    /// <inheritdoc />
    public void Warning(string message, params object?[]? args) =>
        Log(LogLevel.Warning, null, message, args);

    /// <inheritdoc />
    public void Warning(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Warning, exception, message, args);

    #endregion

    #region Error

    /// <inheritdoc />
    public void Error(string message) =>
        Log(LogLevel.Error, null, message, null);

    /// <inheritdoc />
    public void Error(string message, params object?[]? args) =>
        Log(LogLevel.Error, null, message, args);

    /// <inheritdoc />
    public void Error(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Error, exception, message, args);

    #endregion

    #region Fatal

    /// <inheritdoc />
    public void Fatal(string message) =>
        Log(LogLevel.Critical, null, message, null);

    /// <inheritdoc />
    public void Fatal(string message, params object?[]? args) =>
        Log(LogLevel.Critical, null, message, args);

    /// <inheritdoc />
    public void Fatal(Exception? exception, string message, params object?[]? args) =>
        Log(LogLevel.Critical, exception, message, args);

    #endregion

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        Log(logLevel, exception, message, state);
    }

    /// <summary>
    /// Determines whether logging is enabled for the specified log level.
    /// </summary>
    /// <param name="logLevel">The log level to evaluate.</param>
    /// <returns><c>true</c> when the specified level is enabled; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(LogLevelMapper.ToSerilog(logLevel));

    /// <summary>
    /// Begins a logging scope and pushes its value into Serilog log context.
    /// </summary>
    /// <typeparam name="TState">The scope state type.</typeparam>
    /// <param name="state">The scope state to attach to subsequent log events.</param>
    /// <returns>A disposable handle that ends the scope when disposed.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
        Serilog.Context.LogContext.PushProperty("Scope", state, destructureObjects: true);

    private Serilog.ILogger GetContextualLogger()
    {
        var correlationId = OperationContext.Current?.CorrelationId
                            ?? Activity.Current?.GetTagItem("correlation.id")?.ToString();

        return string.IsNullOrWhiteSpace(correlationId)
            ? _logger
            : _logger.ForContext("CorrelationId", correlationId);
    }
}
