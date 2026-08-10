using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Represents the default HTTP error response payload.
/// </summary>
public record ExceptionResponse(
    string Message,
    string ErrorCode,
    string Category,
    string TraceId,
    string? SpanId,
    string? CorrelationId,
    string? Path,
    string? Method,
    string? Endpoint,
    DateTimeOffset TimestampUtc,
    bool IsTransient,
    int? RetryAfterSeconds,
    IReadOnlyCollection<IExceptionDetail>? Details = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null) : IExceptionResponse;
