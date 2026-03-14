using System.Diagnostics;

namespace Raycynix.Extensions.Common.Context;

/// <summary>
/// Represents a context for an operation, encapsulating metadata necessary for tracking
/// and correlating operations across system boundaries. This interface is used to provide
/// consistent access to key information such as correlation IDs, trace IDs, and user IDs.
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// Gets or sets the Correlation ID, which serves as a unique identifier to correlate and track
    /// operations across system boundaries. This property is primarily used to enhance observability,
    /// such as in logging, tracing, or diagnostics, ensuring the ability to trace requests through
    /// distributed systems.
    /// </summary>
    string CorrelationId { get; set; } //TODO: FIX SETTER

    /// <summary>
    /// Gets the unique identifier for the current trace, enabling end-to-end tracking of requests
    /// and operations within a distributed system. This property generally reflects the trace ID
    /// from the current activity in the application's diagnostic context. If no activity exists,
    /// a new trace ID is generated.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// 
    /// </summary>
    string? UserId { get; set; }
}

/// <inheritdoc />
public class OperationContext : IOperationContext
{
    private static readonly AsyncLocal<string> _correlationId = new();
    private static readonly AsyncLocal<string>? _userId = new();

    /// <inheritdoc/>
    public string CorrelationId
    {
        get => _correlationId.Value ??= Guid.NewGuid().ToString();
        set => _correlationId.Value = value;
    }

    /// <inheritdoc />
    public string TraceId => Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

    /// <inheritdoc/>
    public string? UserId
    {
        get => _userId?.Value;
        set => _userId?.Value = value ??= Guid.NewGuid().ToString();
    }
}