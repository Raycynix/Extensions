using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Logging.Internal;

namespace Raycynix.Extensions.Logging.Implementations;

/// <inheritdoc />
public class Logger<T>(Serilog.ILogger logger) : Abstractions.ILogger<T>
{
    private readonly Serilog.ILogger _logger = logger.ForContext<T>();

    /// <inheritdoc />
    public void Log(LogLevel logLevel, string message, Exception? exception = null, object? metadata = null)
    {
        var level = LogLevelMapper.ToSerilog(logLevel);

        if (exception is not null)
        {
            _logger.Write(level, exception, "{Message} {@Metadata}", message, metadata);
            return;
        }

        _logger.Write(level, "{Message} {@Metadata}", message, metadata);
    }

    /// <inheritdoc />
    public void Trace(string message, object? metadata = null) => Log(LogLevel.Trace, message, null, metadata);

    /// <inheritdoc />
    public void Debug(string message, object? metadata = null) => Log(LogLevel.Debug, message, null, metadata);

    /// <inheritdoc />
    public void Information(string message, object? metadata = null) =>
        Log(LogLevel.Information, message, null, metadata);

    /// <inheritdoc />
    public void Warning(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Warning, message, exception, metadata);

    /// <inheritdoc />
    public void Error(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Error, message, exception, metadata);

    /// <inheritdoc />
    public void Fatal(string message, Exception? exception = null, object? metadata = null) =>
        Log(LogLevel.Critical, message, exception, metadata);

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        Log(logLevel, message, exception, state);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(LogLevelMapper.ToSerilog(logLevel));

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
        Serilog.Context.LogContext.PushProperty("Scope", state, destructureObjects: true);
}