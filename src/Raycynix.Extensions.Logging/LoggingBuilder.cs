using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Logging;

/// <summary>
/// Provides a fluent API for adding optional Raycynix logging integrations.
/// </summary>
public class LoggingBuilder(IServiceCollection services, IConfiguration? configuration = null)
{
    /// <summary>
    /// Gets the service collection used by the logging registration.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    
    /// <summary>
    /// Gets the application configuration used to bind logging options when one was supplied.
    /// </summary>
    public IConfiguration? Configuration { get; } = configuration;
}
