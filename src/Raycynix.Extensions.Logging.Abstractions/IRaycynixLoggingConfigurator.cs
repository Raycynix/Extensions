using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Logging.Abstractions.Configurations;
using Serilog;

namespace Raycynix.Extensions.Logging.Abstractions;

/// <summary>
/// Defines an extension point for adding Serilog configuration from optional Raycynix logging packages.
/// </summary>
public interface IRaycynixLoggingConfigurator
{
    /// <summary>
    /// Applies additional Serilog configuration.
    /// </summary>
    /// <param name="context">The host builder context for the application being configured.</param>
    /// <param name="services">The application service provider.</param>
    /// <param name="loggerConfiguration">The Serilog logger configuration to update.</param>
    /// <param name="configuration">The resolved Raycynix logging configuration.</param>
    void Configure(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration,
        LoggingConfiguration configuration);
}
