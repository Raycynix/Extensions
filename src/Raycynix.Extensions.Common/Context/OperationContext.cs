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
    string CorrelationId { get; set; }

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

    /// <summary>
    /// Sets a correlation identifier if it has not been assigned yet.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to assign.</param>
    void SetCorrelationIdIfMissing(string correlationId);
}

/// <inheritdoc />
public class OperationContext : IOperationContext
{
    private string? _correlationId;
    private string? _userId;

    /// <inheritdoc/>
    public string CorrelationId
    {
        get => _correlationId ??= Guid.NewGuid().ToString("N");
        set => _correlationId = string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString("N") : value;
    }

    /// <inheritdoc />
    public string TraceId => Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

    /// <inheritdoc/>
    public string? UserId
    {
        get => _userId;
        set => _userId = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <inheritdoc />
    public void SetCorrelationIdIfMissing(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(_correlationId))
        {
            CorrelationId = correlationId;
        }
    }
}