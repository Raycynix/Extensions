using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Options;
using Raycynix.Extensions.Logging.Implementations;
using Serilog;

namespace Raycynix.Extensions.Logging.Tests.Registration;

/// <summary>
/// Covers dependency registration for the Raycynix logging package.
/// </summary>
public sealed class LoggingRegistrationTests
{
    /// <summary>
    /// Verifies that the typed logger abstraction is registered as a singleton.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_ShouldRegisterTypedLoggerAbstraction()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(_ => Log.Logger);

        services.AddRaycynixLogging();

        var descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(ILogger<>))
            .Subject;

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(Logger<>));
    }

    /// <summary>
    /// Verifies that repeated registration does not duplicate the typed logger abstraction.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(_ => Log.Logger);

        services.AddRaycynixLogging();
        services.AddRaycynixLogging();

        services.Count(service => service.ServiceType == typeof(ILogger<>)).Should().Be(1);
    }

    /// <summary>
    /// Verifies that the typed logger can be resolved from the container after registration.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_ShouldResolveTypedLogger()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(_ => new LoggerConfiguration().CreateLogger());
        services.AddRaycynixLogging();

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<TestCategory>>();

        logger.Should().BeOfType<Logger<TestCategory>>();
    }

    /// <summary>
    /// Verifies that typed logging configuration binds from the standard section and is exposed through DI.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_WithConfiguration_ShouldBindLoggingOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(_ => new LoggerConfiguration().CreateLogger());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LoggingOptions:ServiceName"] = "orders-api",
                ["LoggingOptions:ServiceVersion"] = "1.2.3",
                ["LoggingOptions:MinimumLevel"] = "Debug"
            })
            .Build();

        services.AddRaycynixLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<LoggingOptions>();

        options.ServiceName.Should().Be("orders-api");
        options.ServiceVersion.Should().Be("1.2.3");
        options.MinimumLevel.Should().Be(Microsoft.Extensions.Logging.LogLevel.Debug);
    }

    /// <summary>
    /// Verifies that invalid typed logging configuration is rejected by the option validation pipeline.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_WithInvalidConfiguration_ShouldFailValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(_ => new LoggerConfiguration().CreateLogger());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LoggingOptions:ServiceName"] = "orders-api",
                ["LoggingOptions:ServiceVersion"] = "1.2.3",
                ["LoggingOptions:OutputTemplate"] = string.Empty
            })
            .Build();

        services.AddRaycynixLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var access = () => provider.GetRequiredService<IOptions<LoggingOptions>>().Value;

        access.Should().Throw<OptionsValidationException>()
            .WithMessage("*output template*");
    }

    /// <summary>
    /// Verifies that host logging can still be configured directly from host configuration without DI registration.
    /// </summary>
    [Fact]
    public void UseRaycynixLogging_WithoutServiceRegistration_ShouldUseConfigurationFallback()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LoggingOptions:ServiceName"] = "fallback-api",
                    ["LoggingOptions:ServiceVersion"] = "1.2.3",
                    ["LoggingOptions:MinimumLevel"] = "Debug"
                });
            })
            .UseRaycynixLogging();

        var build = () =>
        {
            using var host = hostBuilder.Build();
        };

        build.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that resolving validated logging options does not create a dependency cycle
    /// while the Serilog provider is being initialized.
    /// </summary>
    [Fact]
    public async Task UseRaycynixLogging_WithServiceRegistration_ShouldStartAndStopHost()
    {
        using var host = Host.CreateDefaultBuilder()
            .UseRaycynixLogging()
            .ConfigureServices((context, services) =>
                services.AddRaycynixLogging(context.Configuration))
            .Build();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await host.StartAsync(timeout.Token);
        await host.StopAsync(timeout.Token);
    }

    private sealed class TestCategory;
}
