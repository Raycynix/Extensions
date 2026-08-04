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
/// Provides a fluent API for extending the Raycynix Serilog pipeline.
/// </summary>
public sealed class RaycynixSerilogBuilder
{
    internal RaycynixSerilogBuilder(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        RaycynixSerilogOptions options)
    {
        Services = services
                   ?? throw new ArgumentNullException(nameof(services));

        Configuration = configuration
                        ?? throw new ArgumentNullException(
                            nameof(configuration));

        Environment = environment
                      ?? throw new ArgumentNullException(
                          nameof(environment));

        Options = options
                  ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the host environment.
    /// </summary>
    public IHostEnvironment Environment { get; }

    /// <summary>
    /// Gets the mutable Raycynix Serilog options.
    /// </summary>
    public RaycynixSerilogOptions Options { get; }

    /// <summary>
    /// Registers a typed Serilog configurator.
    /// </summary>
    public RaycynixSerilogBuilder AddConfigurator<TConfigurator>()
        where TConfigurator :
        class,
        IRaycynixSerilogConfigurator
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IRaycynixSerilogConfigurator,
                TConfigurator>());

        return this;
    }

    /// <summary>
    /// Registers a typed configurator that contributes an output sink.
    /// </summary>
    public RaycynixSerilogBuilder AddSinkConfigurator<TConfigurator>()
        where TConfigurator :
        class,
        IRaycynixSerilogSinkConfigurator
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IRaycynixSerilogConfigurator,
                TConfigurator>());

        return this;
    }

    /// <summary>
    /// Registers an existing configurator instance.
    /// </summary>
    public RaycynixSerilogBuilder AddConfigurator(
        IRaycynixSerilogConfigurator configurator)
    {
        ArgumentNullException.ThrowIfNull(configurator);

        Services.AddSingleton(configurator);

        return this;
    }

    /// <summary>
    /// Registers an inline Serilog configuration callback.
    /// </summary>
    public RaycynixSerilogBuilder ConfigureLogger(
        Action<
            RaycynixSerilogContext,
            global::Serilog.LoggerConfiguration> configure,
        int order = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.AddSingleton<
            IRaycynixSerilogConfigurator>(
            new DelegateRaycynixSerilogConfigurator(
                configure,
                order));

        return this;
    }

    /// <summary>
    /// Registers an inline callback that contributes an output sink.
    /// </summary>
    /// <remarks>
    /// Use this method instead of <see cref="ConfigureLogger"/> when the
    /// callback calls <c>WriteTo</c>. It prevents the fallback console sink
    /// from being added alongside the programmatic sink.
    /// </remarks>
    public RaycynixSerilogBuilder ConfigureSink(
        Action<
            RaycynixSerilogContext,
            global::Serilog.LoggerConfiguration> configure,
        int order = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.AddSingleton<
            IRaycynixSerilogConfigurator>(
            new DelegateRaycynixSerilogSinkConfigurator(
                configure,
                order));

        return this;
    }

    /// <summary>
    /// Registers an arbitrary singleton service that can later be resolved
    /// by a Serilog configurator.
    /// </summary>
    public RaycynixSerilogBuilder AddSingleton<TService>(
        TService instance)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        Services.AddSingleton(instance);

        return this;
    }
}
