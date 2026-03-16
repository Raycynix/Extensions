using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating that access to a resource is forbidden.
/// </summary>
public class ForbiddenException(
    string message = "Access to the requested resource is forbidden.",
    string errorCode = "FORBIDDEN",
    IReadOnlyCollection<IExceptionDetail>? details = null,
    object? secureDetails = null,
    Exception? innerException = null)
    : RaycynixException(
        message,
        errorCode,
        403,
        ErrorCategory.Forbidden,
        details,
        secureDetails,
        innerException);