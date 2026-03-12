using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

public abstract class RaycynixException : Exception, IRaycynixException
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public object? SecureDetails { get; }

    protected RaycynixException(string message, string errorCode, int statusCode) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected RaycynixException(string message, string errorCode, int statusCode = 500, object? secureDetails = null,
        Exception? innerException = null) :
        base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        SecureDetails = secureDetails;
    }

    public virtual object? GetLoggingDetails() => SecureDetails;
}