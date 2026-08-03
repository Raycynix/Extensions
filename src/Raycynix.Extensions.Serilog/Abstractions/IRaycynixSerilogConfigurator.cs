using Raycynix.Extensions.Serilog.Contexts;
using Serilog;

namespace Raycynix.Extensions.Serilog.Abstractions;

/// <summary>
/// Defines an extension point for packages that contribute configuration
/// to the Raycynix Serilog pipeline.
/// </summary>
public interface IRaycynixSerilogConfigurator
{
    /// <summary>
    /// Gets the configurator execution order.
    /// Lower values execute first.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Applies additional configuration to the Serilog pipeline.
    /// </summary>
    /// <param name="loggerConfiguration">
    /// The logger configuration being constructed.
    /// </param>
    /// <param name="context">
    /// The Raycynix Serilog configuration context.
    /// </param>
    void Configure(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogContext context);
}