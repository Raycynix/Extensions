using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Metrics.Tests.Registration;

/// <summary>
/// Covers dependency registration for the metrics package.
/// </summary>
public sealed class MetricsRegistrationTests
{
    [Fact]
    public void AddRaycynixMetrics_ShouldRegisterStandardMeterFactory()
    {
        var services = new ServiceCollection();
        services.AddRaycynixMetrics();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IMeterFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddRaycynixMetrics_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();
        services.AddRaycynixMetrics();
        services.AddRaycynixMetrics();

        using var provider = services.BuildServiceProvider();

        provider.GetServices<IMeterFactory>().Should().ContainSingle();
    }

    [Fact]
    public void AddRaycynixMetrics_ShouldInvokeStandardMetricsConfiguration()
    {
        var services = new ServiceCollection();
        var invoked = false;

        services.AddRaycynixMetrics(_ => invoked = true);

        invoked.Should().BeTrue();
    }

    [Fact]
    public void AddRaycynixMetrics_ShouldNotRegisterHealthChecks()
    {
        var services = new ServiceCollection();

        services.AddRaycynixMetrics();

        services.Should().NotContain(descriptor =>
            descriptor.ServiceType.FullName == "Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService");
    }

    [Fact]
    public void MeterFactories_FromDifferentProviders_ShouldBeIsolated()
    {
        var firstServices = new ServiceCollection();
        var secondServices = new ServiceCollection();
        firstServices.AddRaycynixMetrics();
        secondServices.AddRaycynixMetrics();

        using var firstProvider = firstServices.BuildServiceProvider();
        using var secondProvider = secondServices.BuildServiceProvider();

        var firstMeter = firstProvider.GetRequiredService<IMeterFactory>().Create("raycynix.test.isolation");
        var secondMeter = secondProvider.GetRequiredService<IMeterFactory>().Create("raycynix.test.isolation");

        firstMeter.Should().NotBeSameAs(secondMeter);
    }
}
