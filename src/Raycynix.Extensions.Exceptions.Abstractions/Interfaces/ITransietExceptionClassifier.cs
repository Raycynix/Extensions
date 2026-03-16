namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Determines whether an exception represents a transient failure.
/// </summary>
public interface ITransientExceptionClassifier
{
    /// <summary>
    /// Determines whether the specified exception is transient.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><c>true</c> if the exception is considered transient; otherwise, <c>false</c>.</returns>
    bool IsTransient(Exception exception);
}