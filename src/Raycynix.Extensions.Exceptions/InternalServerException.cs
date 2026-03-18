using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception for an unexpected internal application error.
/// </summary>
public class InternalServerException(
    string message,
    Exception? innerException = null,
    object? secureDetails = null,
    IErrorExecutionContext? executionContext = null)
    : RaycynixException(
        message,
        "INTERNAL_SERVER_ERROR",
        500,
        ErrorCategory.Internal,
        secureDetails: secureDetails,
        innerException: innerException,
        executionContext: executionContext);
