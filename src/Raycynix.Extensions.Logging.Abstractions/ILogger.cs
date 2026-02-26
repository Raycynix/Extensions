using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions;

public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
{
    void Log(LogLevel logLevel, string message, Exception? exception = null, object? metadata = null);
    
    void Trace(string message, object? metadata = null) => Log(LogLevel.Trace, message, null, metadata);
    
    void Debug(string message, object? metadata = null) => Log(LogLevel.Debug, message, null, metadata);
    
    void Information(string message, object? metadata = null) => Log(LogLevel.Information, message, null, metadata);
    
    void Warning(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Warning, message, exception, metadata);
    
    void Error(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Error, message, exception, metadata);
    
    void Fatal(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Critical, message, exception, metadata);
}