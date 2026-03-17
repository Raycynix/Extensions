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
    string? QueryString,
    DateTimeOffset TimestampUtc,
    IReadOnlyCollection<IExceptionDetail>? Details = null,
    IDictionary<string, string[]>? ValidationErrors = null) : IExceptionResponse;
