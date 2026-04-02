using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

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
