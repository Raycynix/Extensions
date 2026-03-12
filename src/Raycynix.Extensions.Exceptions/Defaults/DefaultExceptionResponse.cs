using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

public record DefaultExceptionResponse(
    string Message,
    string ErrorCode,
    string TraceId,
    IDictionary<string, string[]>? ValidationErrors = null) : IExceptionResponse;