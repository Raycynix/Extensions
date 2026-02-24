using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Abstractions
{
    /// <summary>
    /// Represents a Raycynix logger interface that extends the standard <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>.
    /// </summary>
    /// <typeparam name="T">The logging category type.</typeparam>
    public interface ILogger<out T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        // TODO: custom logging contracts or tracing integration.
    }
}
