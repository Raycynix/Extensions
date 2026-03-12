using Raycynix.Extensions.Exceptions.Abstractions;

namespace Raycynix.Extensions.Exceptions.Options;

public class ExceptionMapperOptions
{
    public Dictionary<Type, Func<Exception, RaycynixException>> Mappings { get; } = new();

    public void Map<TException>(Func<TException, RaycynixException> mapper) where TException : Exception 
        => Mappings[typeof(TException)] = ex => mapper((TException)ex);
}