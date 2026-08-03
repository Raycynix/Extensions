using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Configurations;
using Raycynix.Extensions.Serilog.Contexts;
using Raycynix.Extensions.Serilog.Internal;

namespace Raycynix.Extensions.Serilog;

/// <summary>
/// Provides a fluent API for extending Raycynix Serilog registration.
/// </summary>
public sealed class RaycynixSerilogBuilder
{
    internal RaycynixSerilogBuilder(
        IHostApplicationBuilder applicationBuilder,
        RaycynixSerilogOptions options)
    {
        ApplicationBuilder = applicationBuilder
                             ?? throw new ArgumentNullException(
                                 nameof(applicationBuilder));

        Options = options
                  ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the application builder being configured.
    /// </summary>
    public IHostApplicationBuilder ApplicationBuilder { get; }

    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    public IServiceCollection Services => ApplicationBuilder.Services;

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration => ApplicationBuilder.Configuration;

    /// <summary>
    /// Gets the host environment.
    /// </summary>
    public IHostEnvironment Environment => ApplicationBuilder.Environment;

    /// <summary>
    /// Gets the mutable Raycynix Serilog options.
    /// These options are validated after the registration callback completes.
    /// </summary>
    public RaycynixSerilogOptions Options { get; }

    /// <summary>
    /// Registers a typed Serilog configurator.
    /// The configurator can resolve constructor dependencies from DI.
    /// </summary>
    /// <typeparam name="TConfigurator">
    /// Configurator implementation type.
    /// </typeparam>
    /// <returns>The same builder instance.</returns>
    public RaycynixSerilogBuilder AddConfigurator<TConfigurator>()
        where TConfigurator : class, IRaycynixSerilogConfigurator
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IRaycynixSerilogConfigurator,
                TConfigurator>());

        return this;
    }

    /// <summary>
    /// Registers an inline Serilog configuration callback.
    /// </summary>
    /// <param name="configure">
    /// Callback that modifies the logger configuration.
    /// </param>
    /// <param name="order">
    /// Callback execution order. Lower values execute first.
    /// </param>
    /// <returns>The same builder instance.</returns>
    public RaycynixSerilogBuilder ConfigureLogger(
        Action<
            RaycynixSerilogContext,
            global::Serilog.LoggerConfiguration> configure,
        int order = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.AddSingleton<IRaycynixSerilogConfigurator>(
            new DelegateRaycynixSerilogConfigurator(
                configure,
                order));

        return this;
    }
}