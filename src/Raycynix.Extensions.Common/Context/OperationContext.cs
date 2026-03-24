using System.Diagnostics;

namespace Raycynix.Extensions.Common.Context;

/// <summary>
/// Represents per-operation metadata used for correlation, tracing, and request subject identification.
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// Gets or sets the correlation identifier for the current operation.
    /// </summary>
    string CorrelationId { get; set; }

    /// <summary>
    /// Gets the current trace identifier from <see cref="Activity.Current"/>,
    /// or generates a fallback identifier when no activity exists.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets or sets the identifier of the current user, if available.
    /// </summary>
    string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the current request subject, if available.
    /// </summary>
    string? SubjectId { get; set; }

    /// <summary>
    /// Gets or sets the type of the current request subject, if available.
    /// </summary>
    string? SubjectType { get; set; }

    /// <summary>
    /// Assigns the correlation identifier only when it has not been set yet.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to assign.</param>
    void SetCorrelationIdIfMissing(string correlationId);

}

/// <inheritdoc />
public class OperationContext : IOperationContext
{
    private static readonly AsyncLocal<IOperationContext?> _current = new();
    private string? _correlationId;
    private string? _userId;
    private string? _subjectId;
    private string? _subjectType;

    /// <summary>
    /// Gets or sets the ambient operation context for the current async flow.
    /// </summary>
    public static IOperationContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

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
    public string? SubjectId
    {
        get => _subjectId;
        set => _subjectId = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <inheritdoc />
    public string? SubjectType
    {
        get => _subjectType;
        set => _subjectType = string.IsNullOrWhiteSpace(value) ? null : value;
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
