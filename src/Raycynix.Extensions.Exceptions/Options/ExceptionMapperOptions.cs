using Raycynix.Extensions.Exceptions.Abstractions;
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
    public Dictionary<Type, Func<Exception, RaycynixException>> Mappings { get; } = new();

    public void Map<TException>(Func<TException, RaycynixException> mapper) where TException : Exception 
        => Mappings[typeof(TException)] = ex => mapper((TException)ex);
}