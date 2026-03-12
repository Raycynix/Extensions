namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

public interface IRaycynixException
{
    string ErrorCode { get; }
    int StatusCode { get; }
    string Message { get; }

    object? SecureDetails { get; }
}