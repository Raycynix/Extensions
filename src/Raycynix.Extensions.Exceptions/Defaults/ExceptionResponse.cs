using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Represents the default structure of an exception response.
/// </summary>
/// <remarks>
/// This record is used to encapsulate details about an exception in a standardized format,
/// including a message, error code, trace ID, and optional validation error information.
/// </remarks>
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