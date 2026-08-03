using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Configurations;
using Raycynix.Extensions.Serilog.Contexts;
using Raycynix.Extensions.Serilog.Internal;
using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace Raycynix.Extensions.Serilog;

/// <summary>
/// Provides host integration extensions for Raycynix Serilog.
/// </summary>
public static class RaycynixSerilogExtensions
{
    /// <summary>
    /// Configures Serilog using Raycynix conventions and registers it
    /// as the Microsoft.Extensions.Logging provider.
    /// </summary>
    /// <param name="builder">
    /// The host application builder to configure.
    /// </param>
    /// <param name="configure">
    /// Optional callback for adjusting options and adding integrations.
    /// </param>
    /// <returns>The same application builder instance.</returns>
    public static IHostApplicationBuilder AddRaycynixSerilog(
        this IHostApplicationBuilder builder,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        EnsureNotRegistered(builder.Services);

        var options = new RaycynixSerilogOptions();

        builder.Configuration
            .GetSection(RaycynixSerilogOptions.SectionName)
            .Bind(options);

        var raycynixBuilder =
            new RaycynixSerilogBuilder(builder, options);

        configure?.Invoke(raycynixBuilder);

        RaycynixSerilogOptionsValidator
            .ApplyDefaultsAndValidate(
                options,
                builder.Environment);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<RaycynixSerilogRegistrationMarker>();

        builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                var resolvedOptions = services.GetRequiredService<RaycynixSerilogOptions>();

                ApplyRaycynixDefaults(loggerConfiguration, resolvedOptions);

                var configurationReaderOptions = new ConfigurationReaderOptions
                {
                    SectionName = resolvedOptions.SerilogSectionName
                };

                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration, configurationReaderOptions)
                    .ReadFrom.Services(services);

                AddFallbackConsoleIfRequired(builder.Configuration, loggerConfiguration, resolvedOptions);

                var context = new RaycynixSerilogContext(
                    builder.Configuration,
                    builder.Environment,
                    services,
                    resolvedOptions);

                foreach (var configurator in services
                             .GetServices<IRaycynixSerilogConfigurator>()
                             .OrderBy(current => current.Order))
                {
                    configurator.Configure(loggerConfiguration, context);
                }
            },
            preserveStaticLogger:
            options.PreserveStaticLogger,
            writeToProviders:
            options.WriteToProviders);

        return builder;
    }

    private static void ApplyRaycynixDefaults(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options)
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(
                RaycynixSerilogPropertyNames.ServiceName,
                options.ServiceName)
            .Enrich.WithProperty(
                RaycynixSerilogPropertyNames.ServiceVersion,
                options.ServiceVersion)
            .Enrich.WithProperty(
                RaycynixSerilogPropertyNames.Environment,
                options.Environment);

        if (!options.ApplyDefaultLevelOverrides)
            return;


        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);
    }

    private static void AddFallbackConsoleIfRequired(
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options)
    {
        if (!options.UseDefaultConsoleWhenNoSinksConfigured)
            return;

        if (HasConfiguredSinks(
                configuration,
                options.SerilogSectionName))
            return;

        loggerConfiguration.WriteTo.Console(
            outputTemplate:
            options.DefaultConsoleOutputTemplate);
    }

    private static bool HasConfiguredSinks(
        IConfiguration configuration,
        string serilogSectionName)
    {
        var writeToSection = configuration
            .GetSection(serilogSectionName)
            .GetSection("WriteTo");

        return !string.IsNullOrWhiteSpace(writeToSection.Value)
               || writeToSection.GetChildren().Any();
    }

    private static void EnsureNotRegistered(IServiceCollection services)
    {
        var alreadyRegistered =
            services.Any(descriptor => descriptor.ServiceType == typeof(RaycynixSerilogRegistrationMarker));

        if (alreadyRegistered)
        {
            throw new InvalidOperationException(
                "Raycynix Serilog has already been registered. " +
                "Call AddRaycynixSerilog() only once.");
        }
    }
}