using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating a transient infrastructure failure.
/// </summary>
public class TransientFailureException(
    string message = "A temporary failure occurred.",
    string errorCode = "TRANSIENT_FAILURE",
    int statusCode = 503,
    IReadOnlyCollection<IExceptionDetail>? details = null,
    object? secureDetails = null,
    Exception? innerException = null)
    : RaycynixException(
        message,
        errorCode,
        statusCode,
        ErrorCategory.Transient,
        details,
        secureDetails,
        innerException);