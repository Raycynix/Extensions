using Raycynix.Extensions.Exceptions.Abstractions;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating an internal server error.
/// This exception is used to signify issues that occur within the server-side logic
/// and typically result in an HTTP status code of 500 (Internal Server Error).
/// </summary>
/// <remarks>
/// Inherits from <see cref="RaycynixException"/>, providing additional context such as
/// an error code ("INTERNAL_SERVER_ERROR") and a default status code (500).
/// </remarks>
public class
    InternalServerException(string message, Exception? innerException = null)
    : RaycynixException(
        message,
        "INTERNAL_SERVER_ERROR",
        500,
        innerException);