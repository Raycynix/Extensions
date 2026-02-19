using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions
{
    /// <summary>
    /// Represents a Raycynix logger interface that extends the standard <see cref="ILogger{TCategoryName}"/>.
    /// </summary>
    /// <typeparam name="T">The logging category type.</typeparam>
    public interface ICustomLogger<out T> : ILogger<T>
    {
        // TODO: custom logging contracts or tracing integration.
    }
}
