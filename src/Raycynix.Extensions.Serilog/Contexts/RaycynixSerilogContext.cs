using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Configurations;

namespace Raycynix.Extensions.Serilog.Contexts;

/// <summary>
/// Contains host and application services available while configuring Serilog.
/// </summary>
public sealed class RaycynixSerilogContext
{
    /// <summary>
    /// Initializes a new configuration context.
    /// </summary>
    public RaycynixSerilogContext(
        IConfiguration configuration,
        IHostEnvironment environment,
        IServiceProvider services,
        RaycynixSerilogOptions options)
    {
        Configuration = configuration
                        ?? throw new ArgumentNullException(nameof(configuration));

        Environment = environment
                      ?? throw new ArgumentNullException(nameof(environment));

        Services = services
                   ?? throw new ArgumentNullException(nameof(services));

        Options = options
                  ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the current host environment.
    /// </summary>
    public IHostEnvironment Environment { get; }

    /// <summary>
    /// Gets the application service provider.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the resolved Raycynix Serilog options.
    /// </summary>
    public RaycynixSerilogOptions Options { get; }
}