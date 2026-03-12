using Raycynix.Extensions.Exceptions.Abstractions;

namespace Raycynix.Extensions.Exceptions;

public class
    InternalServerException(string message, Exception? innerException = null)
    : RaycynixException(
        message,
        "INTERNAL_SERVER_ERROR",
        500,
        innerException);