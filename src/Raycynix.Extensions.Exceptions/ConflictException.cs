using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating a conflict with the current state of a resource.
/// </summary>
public class ConflictException(
    string message,
    string errorCode = "RESOURCE_CONFLICT",
    IReadOnlyCollection<IExceptionDetail>? details = null,
    object? secureDetails = null,
    Exception? innerException = null)
    : RaycynixException(
        message,
        errorCode,
        409,
        ErrorCategory.Conflict,
        details,
        secureDetails,
        innerException);