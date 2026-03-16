using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating that authentication is required or has failed.
/// </summary>
public class UnauthorizedException(
    string message = "Authentication is required.",
    string errorCode = "UNAUTHORIZED",
    IReadOnlyCollection<IExceptionDetail>? details = null,
    object? secureDetails = null,
    Exception? innerException = null)
    : RaycynixException(
        message,
        errorCode,
        401,
        ErrorCategory.Unauthorized,
        details,
        secureDetails,
        innerException);