using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Provides a default implementation of the <see cref="IExceptionMapper"/> interface
/// to map exceptions to the custom <see cref="RaycynixException"/> type.
/// </summary>
/// <remarks>
/// This class enables handling of custom exception mapping logic based on a predefined
/// set of mappings. If an exception type is not mapped explicitly, it falls back to
/// creating an <see cref="InternalServerException"/> to handle generic, unhandled errors.
/// </remarks>
public class DefaultExceptionMapper(IReadOnlyDictionary<Type, Func<Exception, RaycynixException>> mappings)
    : IExceptionMapper
{
    /// <summary>
    /// Maps a given exception to a <see cref="RaycynixException"/> instance based on predefined mappings.
    /// </summary>
    /// <param name="ex">The exception to be mapped. This can be any exception derived from <see cref="Exception"/>.</param>
    /// <return>
    /// A <see cref="RaycynixException"/> instance. If the input exception is already a <see cref="RaycynixException"/> instance,
    /// it is returned as is. If a mapping is defined for the exception type, the mapped exception is returned. Otherwise,
    /// a default <see cref="InternalServerException"/> is returned.
    /// </return>
    public RaycynixException Map(Exception ex)
    {
        if (ex is RaycynixException rayEx) return rayEx;

        var exceptionType = ex.GetType();

        var mapping = mappings
            .Where(x => x.Key.IsAssignableFrom(exceptionType))
            .OrderByDescending(x => GetInheritanceDepth(x.Key))
            .Select(x => x.Value)
            .FirstOrDefault();

        return mapping is not null
            ? mapping(ex)
            : new InternalServerException("An unhandled error occurred.", ex);
    }

    private static int GetInheritanceDepth(Type type)
    {
        var depth = 0;
        var current = type;

        while (current.BaseType is not null)
        {
            depth++;
            current = current.BaseType;
        }

        return depth;
    }
}