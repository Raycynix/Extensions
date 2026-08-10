using System.Collections.ObjectModel;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Options;

/// <summary>
/// Provides configuration for custom exception mappings.
/// </summary>
public class ExceptionMapperOptions
{
    private readonly Dictionary<Type, Func<Exception, RaycynixException>> _mappings = new();

    /// <summary>
    /// Gets the registered exception mappings.
    /// </summary>
    public IReadOnlyDictionary<Type, Func<Exception, RaycynixException>> Mappings =>
        new ReadOnlyDictionary<Type, Func<Exception, RaycynixException>>(_mappings);

    /// <summary>
    /// Registers a custom mapping for an exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to map.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    public void Map<TException>(Func<TException, RaycynixException> mapper) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mappings[typeof(TException)] = ex =>
            mapper((TException)ex) ??
            throw new InvalidOperationException(
                $"Exception mapper for '{typeof(TException).FullName}' returned null.");
    }

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
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "Mapped exception status code must be between 400 and 599.");
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown error category.");
        }

        _mappings[typeof(TException)] = ex => new MappedException(
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
