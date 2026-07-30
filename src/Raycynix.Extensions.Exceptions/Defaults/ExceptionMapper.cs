using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Maps arbitrary exceptions to <see cref="RaycynixException"/> instances.
/// </summary>
public class ExceptionMapper : IExceptionMapper
{
    private readonly IReadOnlyDictionary<Type, Func<Exception, RaycynixException>> _mappings;

    /// <summary>
    /// Initializes the default exception mapper.
    /// </summary>
    public ExceptionMapper(IReadOnlyDictionary<Type, Func<Exception, RaycynixException>> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        _mappings = new Dictionary<Type, Func<Exception, RaycynixException>>(mappings);
    }

    /// <summary>
    /// Maps an exception to a Raycynix exception.
    /// </summary>
    /// <param name="ex">The exception to map.</param>
    /// <returns>The mapped <see cref="RaycynixException"/> instance.</returns>
    public RaycynixException Map(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        if (ex is RaycynixException rayEx) return rayEx;

        var exceptionType = ex.GetType();

        var mapping = _mappings
            .Where(x => x.Key.IsAssignableFrom(exceptionType))
            .OrderByDescending(x => GetInheritanceDepth(x.Key))
            .Select(x => x.Value)
            .FirstOrDefault();

        return mapping is not null
            ? mapping(ex)
            : new InternalServerException("An unhandled error occurred.", ex, new
            {
                ExceptionType = ex.GetType().FullName,
                ex.Message,
                InnerException = ex.InnerException?.Message
            });
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
