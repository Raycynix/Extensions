using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Configurations;
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
        services.AddSingleton<Serilog.ILogger>(_ => Log.Logger);

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
        services.AddSingleton<Serilog.ILogger>(_ => Log.Logger);

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
        services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration().CreateLogger());
        services.AddRaycynixLogging();

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<TestCategory>>();

        logger.Should().BeOfType<Logger<TestCategory>>();
    }

    /// <summary>
    /// Verifies that typed logging configuration binds from the standard section and is exposed through DI.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_WithConfiguration_ShouldBindLoggingConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration().CreateLogger());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LoggingConfiguration:ServiceName"] = "orders-api",
                ["LoggingConfiguration:ServiceVersion"] = "1.2.3",
                ["LoggingConfiguration:MinimumLevel"] = "Debug"
            })
            .Build();

        services.AddRaycynixLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<LoggingConfiguration>();

        options.ServiceName.Should().Be("orders-api");
        options.ServiceVersion.Should().Be("1.2.3");
        options.MinimumLevel.Should().Be(Microsoft.Extensions.Logging.LogLevel.Debug);
    }

    /// <summary>
    /// Verifies that invalid typed logging configuration is rejected by the options validation pipeline.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_WithInvalidConfiguration_ShouldFailValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration().CreateLogger());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LoggingConfiguration:ServiceName"] = "orders-api",
                ["LoggingConfiguration:ServiceVersion"] = "1.2.3",
                ["LoggingConfiguration:UseElastic"] = "true",
                ["LoggingConfiguration:ElasticUrl"] = "not-a-url"
            })
            .Build();

        services.AddRaycynixLogging(configuration);

        using var provider = services.BuildServiceProvider();
        var access = () => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LoggingConfiguration>>().Value;

        access.Should().Throw<OptionsValidationException>()
            .WithMessage("*Elasticsearch URL*");
    }

    private sealed class TestCategory;
}
