using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Options;

/// <summary>
/// Provides configuration for custom exception mappings.
/// </summary>
public class ExceptionMapperOptions
{
    /// <summary>
    /// Gets the registered exception mappings.
    /// </summary>
    public Dictionary<Type, Func<Exception, RaycynixException>> Mappings { get; } = new();

    /// <summary>
    /// Registers a custom mapping for an exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to map.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    public void Map<TException>(Func<TException, RaycynixException> mapper) where TException : Exception
        => Mappings[typeof(TException)] = ex => mapper((TException)ex);

    /// <summary>
    /// Registers a simple mapping for an exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to map.</typeparam>
    /// <param name="errorCode">The resulting error code.</param>
    /// <param name="message">The resulting public message.</param>
    /// <param name="statusCode">The resulting status code.</param>
    /// <param name="category">The resulting error category.</param>
    public void Map<TException>(
        string errorCode,
        string message,
        int statusCode,
        ErrorCategory category) where TException : Exception
    {
        Mappings[typeof(TException)] = ex => new MappedException(
            message,
            errorCode,
            statusCode,
            category,
            secureDetails: new
            {
                ExceptionType = ex.GetType().FullName,
                ex.Message
            },
            innerException: ex);
    }

    private sealed class MappedException(
        string message,
        string errorCode,
        int statusCode,
        ErrorCategory category,
        IReadOnlyCollection<IExceptionDetail>? details = null,
        object? secureDetails = null,
        Exception? innerException = null)
        : RaycynixException(
            message,
            errorCode,
            statusCode,
            category,
            details,
            secureDetails,
            innerException);
}
