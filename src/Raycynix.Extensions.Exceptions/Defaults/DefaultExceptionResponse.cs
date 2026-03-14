using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Represents the default structure of an exception response.
/// </summary>
/// <remarks>
/// This record is used to encapsulate details about an exception in a standardized format,
/// including a message, error code, trace ID, and optional validation error information.
/// </remarks>
public record DefaultExceptionResponse(
    string Message,
    string ErrorCode,
    string TraceId,
    IDictionary<string, string[]>? ValidationErrors = null) : IExceptionResponse;