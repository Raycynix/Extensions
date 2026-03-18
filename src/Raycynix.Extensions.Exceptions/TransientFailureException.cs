using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating a transient infrastructure failure.
/// </summary>
public class TransientFailureException : RaycynixException
{
    /// <summary>
    /// Gets the logical operation name associated with the failure.
    /// </summary>
    public string? OperationName { get; }

    /// <summary>
    /// Gets the number of attempts that were performed.
    /// </summary>
    public int? AttemptCount { get; }

    /// <summary>
    /// Gets the maximum number of attempts that were allowed.
    /// </summary>
    public int? MaxAttempts { get; }

    /// <summary>
    /// Gets the recommended retry delay in seconds.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="TransientFailureException"/>.
    /// </summary>
    public TransientFailureException(
        string message = "A temporary failure occurred.",
        string errorCode = "TRANSIENT_FAILURE",
        int statusCode = 503,
        IReadOnlyCollection<IExceptionDetail>? details = null,
        object? secureDetails = null,
        Exception? innerException = null,
        string? operationName = null,
        int? attemptCount = null,
        int? maxAttempts = null,
        int? retryAfterSeconds = null,
        IErrorExecutionContext? executionContext = null)
        : base(
            message,
            errorCode,
            statusCode,
            ErrorCategory.Transient,
            details,
            secureDetails,
            innerException,
            executionContext)
    {
        OperationName = operationName;
        AttemptCount = attemptCount;
        MaxAttempts = maxAttempts;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
