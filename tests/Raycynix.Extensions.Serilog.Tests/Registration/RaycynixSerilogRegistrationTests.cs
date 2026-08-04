using FluentAssertions;
using global::Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;

namespace Raycynix.Extensions.Serilog.Tests.Registration;

public sealed class RaycynixSerilogRegistrationTests
{
    [Fact]
    public void AddRaycynixSerilog_ShouldSupportHostApplicationBuilder()
    {
        var sink = new CollectingSink();
        var builder = TestHostBuilderFactory.Create();

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

            logging.ConfigureLogger((_, loggerConfiguration) =>
                loggerConfiguration.WriteTo.Sink(sink));
        });

        using var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Modern host registration");

        var entry = sink.Single("Modern host registration");

        entry.Level.Should().Be(LogEventLevel.Information);
    }

    [Fact]
    public void UseRaycynixSerilog_ShouldSupportClassicHostBuilder()
    {
        var sink = new CollectingSink();

        using var host = Host
            .CreateDefaultBuilder([])
            .UseEnvironment("Testing")
            .UseRaycynixSerilog(logging =>
            {
                logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

                logging.ConfigureLogger((_, loggerConfiguration) => loggerConfiguration.WriteTo.Sink(sink));
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Classic host registration");

        sink.Single("Classic host registration")
            .Level.Should()
            .Be(LogEventLevel.Information);
    }

    [Fact]
    public void AddRaycynixSerilog_ShouldSupportDirectServiceCollection()
    {
        var sink = new CollectingSink();
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder().Build();

        var environment = new TestHostEnvironment();

        services.AddRaycynixSerilog(
            configuration,
            environment,
            logging =>
            {
                logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

                logging.ConfigureLogger((_, loggerConfiguration) => loggerConfiguration.WriteTo.Sink(sink));
            });

        using var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Direct service registration");

        sink.Single("Direct service registration")
            .Level.Should()
            .Be(LogEventLevel.Information);
    }

    [Fact]
    public void AddRaycynixSerilog_WhenRegisteredTwice_ShouldThrow()
    {
        var builder = TestHostBuilderFactory.Create();

        builder.AddRaycynixSerilog(logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; });

        var registration = () => builder.AddRaycynixSerilog();

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already been registered*");
    }

    [Fact]
    public void UseRaycynixSerilog_WhenRegisteredTwice_ShouldThrowDuringBuild()
    {
        var builder = Host
            .CreateDefaultBuilder([])
            .UseRaycynixSerilog(logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; })
            .UseRaycynixSerilog(logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; });

        var build = builder.Build;

        build.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already been registered*");
    }

    [Fact]
    public void DirectRegistration_WhenRegisteredTwice_ShouldThrow()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var environment = new TestHostEnvironment();

        services.AddRaycynixSerilog(
            configuration,
            environment,
            logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; });

        var registration = () =>
            services.AddRaycynixSerilog(
                configuration,
                environment);

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already been registered*");
    }

    private sealed class TestCategory;
}