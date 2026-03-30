using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.Tests.Service;

/// <summary>
/// Covers runtime creation and basic usage of metrics instruments.
/// </summary>
public sealed class MetricsServiceTests
{
    /// <summary>
    /// Verifies that counters can be created and incremented through the shared abstraction.
    /// </summary>
    [Fact]
    public void CreateCounter_ShouldReturnWorkingCounter()
    {
        var metrics = CreateMetricsService();
        var name = CreateMetricName("counter");

        var counter = metrics.CreateCounter(name, "Test counter", "tenant");
        var act = () => counter.Increment(2, "alpha");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that gauges support set, increment, and decrement operations.
    /// </summary>
    [Fact]
    public void CreateGauge_ShouldReturnWorkingGauge()
    {
        var metrics = CreateMetricsService();
        var name = CreateMetricName("gauge");

        var gauge = metrics.CreateGauge(name, "Test gauge", "tenant");
        var act = () =>
        {
            gauge.Set(10, "alpha");
            gauge.Increment(2, "alpha");
            gauge.Decrement(1, "alpha");
        };

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that histograms support explicit observation and duration measurement.
    /// </summary>
    [Fact]
    public void CreateHistogram_ShouldReturnWorkingHistogram()
    {
        var metrics = CreateMetricsService();
        var name = CreateMetricName("histogram");

        var histogram = metrics.CreateHistogram(name, "Test histogram", "tenant");
        var act = () =>
        {
            histogram.Observe(0.25, "alpha");
            using var _ = histogram.MeasureDuration("alpha");
        };

        act.Should().NotThrow();
    }

    private static IMetricsService CreateMetricsService()
    {
        var services = new ServiceCollection();
        services.AddRaycynixMetrics();

        return services.BuildServiceProvider().GetRequiredService<IMetricsService>();
    }

    private static string CreateMetricName(string prefix)
    {
        return $"raycynix_{prefix}_{Guid.NewGuid():N}";
    }
}
