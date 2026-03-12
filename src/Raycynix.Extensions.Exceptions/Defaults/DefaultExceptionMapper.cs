using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

public class DefaultExceptionMapper(IReadOnlyDictionary<Type, Func<Exception, RaycynixException>> mappings)
    : IExceptionMapper
{
    public RaycynixException Map(Exception ex)
    {
        if (ex is RaycynixException rayEx) return rayEx;

        return mappings.TryGetValue(ex.GetType(), out var mapper)
            ? mapper(ex)
            : new InternalServerException("An unhandled error occurred.", ex);
    }
}