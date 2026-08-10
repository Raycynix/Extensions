using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Configurations;
using Raycynix.Extensions.Serilog.Contexts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace Raycynix.Extensions.Serilog.Internal;

internal static class RaycynixSerilogRegistration
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<RaycynixSerilogBuilder>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        EnsureNotRegistered(services);
        
        services.AddLogging();

        var options = CreateOptions(configuration);

        var builder = new RaycynixSerilogBuilder(
            services,
            configuration,
            environment,
            options);

        configure?.Invoke(builder);

        RaycynixSerilogOptionsValidator.ApplyDefaultsAndValidate(options, environment);

        services.AddSingleton(options);
        services.AddSingleton<RaycynixSerilogRegistrationMarker>();

        RegisterSerilogProvider(
            services,
            configuration,
            environment,
            options);
    }

    private static RaycynixSerilogOptions CreateOptions(
        IConfiguration configuration)
    {
        var options = new RaycynixSerilogOptions();

        configuration
            .GetSection(RaycynixSerilogOptions.SectionName)
            .Bind(options);

        return options;
    }

    private static void RegisterSerilogProvider(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        RaycynixSerilogOptions options)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) =>
            {
                ConfigureLogger(
                    serviceProvider,
                    configuration,
                    environment,
                    loggerConfiguration,
                    options);
            },
            preserveStaticLogger: options.PreserveStaticLogger,
            writeToProviders: options.WriteToProviders);
    }

    private static void ConfigureLogger(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options)
    {
        ApplyRaycynixDefaults(
            loggerConfiguration,
            options);

        ReadNativeSerilogConfiguration(
            configuration,
            loggerConfiguration,
            options);

        loggerConfiguration.ReadFrom.Services(services);

        var context = new RaycynixSerilogContext(
            configuration,
            environment,
            services,
            options);

        var configurators = services
            .GetServices<IRaycynixSerilogConfigurator>()
            .OrderBy(current => current.Order)
            .ToArray();

        ApplyConfigurators(
            configurators,
            loggerConfiguration,
            context);

        AddFallbackConsoleIfRequired(
            services,
            configuration,
            loggerConfiguration,
            options,
            configurators);
    }

    private static void ApplyRaycynixDefaults(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options)
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(RaycynixSerilogPropertyNames.ServiceName, options.ServiceName)
            .Enrich.WithProperty(RaycynixSerilogPropertyNames.ServiceVersion, options.ServiceVersion)
            .Enrich.WithProperty(RaycynixSerilogPropertyNames.Environment, options.Environment);

        if (!options.ApplyDefaultLevelOverrides)
            return;

        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);
    }

    private static void ReadNativeSerilogConfiguration(
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options)
    {
        var readerOptions = new ConfigurationReaderOptions
        {
            SectionName = options.SerilogSectionName
        };

        loggerConfiguration.ReadFrom.Configuration(
            configuration,
            readerOptions);
    }

    private static void ApplyConfigurators(
        IEnumerable<IRaycynixSerilogConfigurator> configurators,
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogContext context)
    {
        foreach (var configurator in configurators)
        {
            configurator.Configure(
                loggerConfiguration,
                context);
        }
    }

    private static void AddFallbackConsoleIfRequired(
        IServiceProvider services,
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogOptions options,
        IEnumerable<IRaycynixSerilogConfigurator> configurators)
    {
        if (!options.UseDefaultConsoleWhenNoSinksConfigured)
            return;

        if (HasConfiguredSinks(configuration, options.SerilogSectionName))
            return;

        if (HasDependencyInjectedSink(services))
            return;

        if (configurators
            .OfType<IRaycynixSerilogSinkConfigurator>()
            .Any(current => current.IsEnabled))
            return;

        loggerConfiguration.WriteTo.Console(outputTemplate: options.DefaultConsoleOutputTemplate);
    }

    private static bool HasDependencyInjectedSink(IServiceProvider services)
    {
        var serviceProbe = services.GetService<IServiceProviderIsService>();

        return serviceProbe is not null
            ? serviceProbe.IsService(typeof(ILogEventSink))
            : services.GetServices<ILogEventSink>().Any();
    }

    private static bool HasConfiguredSinks(
        IConfiguration configuration,
        string serilogSectionName)
    {
        var writeToSection = configuration
            .GetSection(serilogSectionName)
            .GetSection("WriteTo");

        if (!string.IsNullOrWhiteSpace(writeToSection.Value))
            return true;

        return writeToSection
            .GetChildren()
            .Any();
    }

    private static void EnsureNotRegistered(IServiceCollection services)
    {
        var alreadyRegistered = services.Any(descriptor =>
            descriptor.ServiceType == typeof(RaycynixSerilogRegistrationMarker));

        if (alreadyRegistered)
            throw new InvalidOperationException(
                "Raycynix Serilog has already been registered. Register it only once for each service collection.");
    }
}
