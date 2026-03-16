using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Provides a default implementation for classifying transient failures.
/// </summary>
public class TransientExceptionClassifier : ITransientExceptionClassifier
{
    /// <summary>
    /// Determines whether the specified exception is transient.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><c>true</c> if the exception is considered transient; otherwise, <c>false</c>.</returns>
    public bool IsTransient(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            OperationCanceledException => false,
            // TaskCanceledException => true,
            IOException => true,
            HttpRequestException => true,
            TransientFailureException => true,
            AggregateException aggregateException => aggregateException.InnerExceptions.Any(IsTransient),
            _ when HasTransientInnerException(exception) => true,
            _ => false
        };
    }

    private bool HasTransientInnerException(Exception exception)
    {
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        var current = exception.InnerException;

        while (current is not null && visited.Add(current))
        {
            if (IsTransient(current))
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }
}