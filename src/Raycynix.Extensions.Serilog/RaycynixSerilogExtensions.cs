using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Internal;

namespace Raycynix.Extensions.Serilog;

/// <summary>
/// Provides registration extensions for Raycynix Serilog.
/// </summary>
public static class RaycynixSerilogExtensions
{
    /// <summary>
    /// Adds Raycynix Serilog to a modern host application builder.
    /// </summary>
    /// <remarks>
    /// Supports HostApplicationBuilder and WebApplicationBuilder because
    /// both expose IHostApplicationBuilder.
    /// </remarks>
    public static IHostApplicationBuilder AddRaycynixSerilog(
        this IHostApplicationBuilder builder,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        RaycynixSerilogRegistration.Register(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            configure);

        return builder;
    }

    /// <summary>
    /// Adds Raycynix Serilog to the classic generic host builder.
    /// </summary>
    /// <remarks>
    /// The configuration callback executes when the host builder applies
    /// its service registrations.
    /// </remarks>
    public static IHostBuilder UseRaycynixSerilog(
        this IHostBuilder builder,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices(
            (context, services) =>
            {
                RaycynixSerilogRegistration.Register(
                    services,
                    context.Configuration,
                    context.HostingEnvironment,
                    configure);
            });

        return builder;
    }

    /// <summary>
    /// Adds Raycynix Serilog directly to a service collection.
    /// </summary>
    /// <remarks>
    /// This overload is intended for custom hosting models, integration
    /// tests, and applications that manage their own service collection.
    /// </remarks>
    public static IServiceCollection AddRaycynixSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        RaycynixSerilogRegistration.Register(
            services,
            configuration,
            environment,
            configure);

        return services;
    }
}