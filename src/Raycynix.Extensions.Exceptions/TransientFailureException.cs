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
        if (operationName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        }

        if (attemptCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attemptCount),
                attemptCount,
                "Attempt count must be greater than zero when specified.");
        }

        if (maxAttempts is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts),
                maxAttempts,
                "Maximum attempts must be greater than zero when specified.");
        }

        if (attemptCount > maxAttempts)
        {
            throw new ArgumentException(
                "Attempt count cannot be greater than maximum attempts.",
                nameof(attemptCount));
        }

        if (retryAfterSeconds is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retryAfterSeconds),
                retryAfterSeconds,
                "Retry delay cannot be negative.");
        }

        OperationName = operationName;
        AttemptCount = attemptCount;
        MaxAttempts = maxAttempts;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
