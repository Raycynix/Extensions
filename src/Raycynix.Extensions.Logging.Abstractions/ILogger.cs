using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions;

/// <summary>
/// Represents a Raycynix logger interface />.
/// </summary>
/// <typeparam name="T">The logging category type.</typeparam>
public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="metadata"></param>
    void Log(LogLevel logLevel, string message, Exception? exception = null, object? metadata = null);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="metadata"></param>
    void Trace(string message, object? metadata = null) => Log(LogLevel.Trace, message, null, metadata);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="metadata"></param>
    void Debug(string message, object? metadata = null) => Log(LogLevel.Debug, message, null, metadata);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="metadata"></param>
    void Information(string message, object? metadata = null) => Log(LogLevel.Information, message, null, metadata);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="metadata"></param>
    void Warning(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Warning, message, exception, metadata);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="metadata"></param>
    void Error(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Error, message, exception, metadata);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="metadata"></param>
    void Fatal(string message, Exception? exception = null, object? metadata = null) => Log(LogLevel.Critical, message, exception, metadata);
    
    //TODO: Create Documentation
}