using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Logging.Internal;

namespace Raycynix.Extensions.Logging.Implementation;

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