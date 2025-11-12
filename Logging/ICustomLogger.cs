using Microsoft.Extensions.Logging;

namespace Extensions.Logging
{
    public interface ICustomLogger<T> : ILogger<T>
    {
    }
}
