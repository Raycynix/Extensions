using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Metrics.Configurations;

namespace Raycynix.Extensions.Metrics.Tests.Registration;

/// <summary>
/// Covers dependency registration for the metrics package.
/// </summary>
public sealed class MetricsRegistrationTests
{
    /// <summary>
    /// Verifies that the metrics service is registered as a singleton.
    /// </summary>
    [Fact]
    public void AddRaycynixMetrics_ShouldRegisterMetricsService()
    {
        var services = new ServiceCollection();

        services.AddRaycynixMetrics();

        var descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(IMetricsService))
            .Subject;

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().NotBeNull();
        descriptor.ImplementationType!.Name.Should().Be("MetricsService");
    }

    /// <summary>
    /// Verifies that repeated registration does not duplicate the metrics service.
    /// </summary>
    [Fact]
    public void AddRaycynixMetrics_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddRaycynixMetrics();
        services.AddRaycynixMetrics();

        services.Count(service => service.ServiceType == typeof(IMetricsService)).Should().Be(1);
    }

    /// <summary>
    /// Verifies that the optional health check setup hook is executed.
    /// </summary>
    [Fact]
    public void AddRaycynixMetrics_ShouldInvokeHealthSetup()
    {
        var services = new ServiceCollection();
        var invoked = false;

        services.AddRaycynixMetrics(health =>
        {
            invoked = true;
            health.AddCheck<TestHealthCheck>("metrics-test");
        });

        invoked.Should().BeTrue();
        services.Any(service => service.ServiceType == typeof(HealthCheckService)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that typed metrics configuration binds from the standard section and is exposed through DI.
    /// </summary>
    [Fact]
    public void AddRaycynixMetrics_WithConfiguration_ShouldBindMetricsConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MetricsConfiguration:UsePrometheus"] = "false",
                ["MetricsConfiguration:MetricsEndpoint"] = "/internal/metrics",
                ["MetricsConfiguration:UseHealthChecks"] = "false"
            })
            .Build();

        services.AddRaycynixMetrics(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<MetricsConfiguration>();

        options.UsePrometheus.Should().BeFalse();
        options.MetricsEndpoint.Should().Be("/internal/metrics");
        options.UseHealthChecks.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that invalid metrics configuration is rejected by the options validation pipeline.
    /// </summary>
    [Fact]
    public void AddRaycynixMetrics_WithInvalidConfiguration_ShouldFailValidation()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MetricsConfiguration:MetricsEndpoint"] = "metrics"
            })
            .Build();

        services.AddRaycynixMetrics(configuration);

        using var provider = services.BuildServiceProvider();
        var access = () => provider.GetRequiredService<IOptions<MetricsConfiguration>>().Value;

        access.Should().Throw<OptionsValidationException>()
            .WithMessage("*must start with '/'*");
    }

    private sealed class TestHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
