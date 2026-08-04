using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.Tests.Service;

/// <summary>
/// Covers standard .NET instrument creation and measurement collection.
/// </summary>
public sealed class MetricsInstrumentationTests
{
    [Fact]
    public void RaycynixMeter_ShouldPublishCounterMeasurements_WithNamedTags()
    {
        using var provider = CreateProvider();
        var measurements = new List<(long Value, KeyValuePair<string, object?>[] Tags)>();
        using var listener = CreateListener();
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
            measurements.Add((value, tags.ToArray())));
        listener.Start();

        var meter = RaycynixMetrics.CreateMeter(provider.GetRequiredService<IMeterFactory>());
        var counter = meter.CreateCounter<long>("raycynix.test.orders", "{order}");
        counter.Add(2, new KeyValuePair<string, object?>("raycynix.status", "success"));

        measurements.Should().ContainSingle();
        measurements[0].Value.Should().Be(2);
        measurements[0].Tags.Should().ContainSingle(tag =>
            tag.Key == "raycynix.status" && Equals(tag.Value, "success"));
    }

    [Fact]
    public void MeasureDuration_ShouldRecordPositiveSeconds_OnlyOnce()
    {
        using var provider = CreateProvider();
        var measurements = new List<double>();
        using var listener = CreateListener();
        listener.SetMeasurementEventCallback<double>((_, value, _, _) => measurements.Add(value));
        listener.Start();

        var meter = RaycynixMetrics.CreateMeter(provider.GetRequiredService<IMeterFactory>());
        var histogram = meter.CreateHistogram<double>("raycynix.test.duration", "s");
        var timer = histogram.MeasureDuration(
            new KeyValuePair<string, object?>("raycynix.operation", "test"));

        timer.Dispose();
        timer.Dispose();

        measurements.Should().ContainSingle();
        measurements[0].Should().BeGreaterThanOrEqualTo(0);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddRaycynixMetrics();
        return services.BuildServiceProvider();
    }

    private static MeterListener CreateListener()
    {
        return new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == RaycynixMetrics.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };
    }
}
