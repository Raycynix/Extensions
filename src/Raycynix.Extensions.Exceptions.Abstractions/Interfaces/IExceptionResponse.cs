namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

public interface IExceptionResponse
{
    string Message  { get; }
    string ErrorCode { get; }
    string TraceId { get; }
    IDictionary<string, object?> ValidationErrors { get; }
}