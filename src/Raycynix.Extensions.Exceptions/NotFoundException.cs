using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating that a requested resource was not found.
/// </summary>
public class NotFoundException(
    string message,
    string errorCode = "RESOURCE_NOT_FOUND",
    IReadOnlyCollection<IExceptionDetail>? details = null,
    object? secureDetails = null,
    Exception? innerException = null)
    : RaycynixException(
        message,
        errorCode,
        404,
        ErrorCategory.NotFound,
        details,
        secureDetails,
        innerException);