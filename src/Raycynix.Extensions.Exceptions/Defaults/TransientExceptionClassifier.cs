using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Classifies exceptions that are likely to succeed on retry.
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
            TaskCanceledException => true,
            OperationCanceledException => false,
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

    private sealed class ReferenceEqualityComparer : IEqualityComparer<Exception>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public bool Equals(Exception? x, Exception? y) => ReferenceEquals(x, y);

        public int GetHashCode(Exception obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
