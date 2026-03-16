using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Options;

/// <summary>
/// Provides configuration options for mapping exceptions to <see cref="RaycynixException"/> instances.
/// </summary>
/// <remarks>
/// This class allows the registration of custom exception mappings that associate specific exception
/// types with transformation logic to produce instances of <see cref="RaycynixException"/>.
/// These mappings are used by an <see cref="IExceptionMapper"/> implementation to handle and transform
/// exceptions during runtime.
/// </remarks>
public class ExceptionMapperOptions
{
    /// <summary>
    /// Gets the collection of mappings that associate exception types with functions
    /// for transforming them into <see cref="RaycynixException"/> instances.
    /// </summary>
    /// <remarks>
    /// This property stores a dictionary where the key represents the type of the exception
    /// to be mapped, and the value is a function defining how to transform an exception
    /// to that type into a <see cref="RaycynixException"/>. These mappings are utilized
    /// by the <see cref="IExceptionMapper"/> to customize exception handling behavior.
    /// </remarks>
    public Dictionary<Type, Func<Exception, RaycynixException>> Mappings { get; } = new();

    /// <summary>
    /// Registers a mapping for a specific exception type to transform it into a <see cref="RaycynixException"/> instance.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to map. Must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="mapper">
    /// A function that converts an exception of type <typeparamref name="TException"/> into an instance of <see cref="RaycynixException"/>.
    /// </param>
    public void Map<TException>(Func<TException, RaycynixException> mapper) where TException : Exception
        => Mappings[typeof(TException)] = ex => mapper((TException)ex);

    /// <summary>
    /// Registers a simple mapping for a specific exception type.
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