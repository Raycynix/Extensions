using System.Diagnostics;
using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Database.Tests.Observability;

/// <summary>
/// Covers observability integration for database operations.
/// </summary>
public sealed class DatabaseObservabilityTests
{
    /// <summary>
    /// Verifies that observability works without tracing or metrics registrations.
    /// </summary>
    [Fact]
    public void Constructor_ShouldWorkWithoutOptionalDependencies()
    {
        var observability = CreateObservability(new ServiceCollection().BuildServiceProvider());

        var act = () =>
        {
            using var operation = InvokeBeginOperation(observability, "sqlite", "initialization");
            InvokeRecordSuccess(observability, "sqlite", "initialization");
            InvokeRecordFailure(observability, "sqlite", "initialization");
            InvokeAddTag(observability, "database.provider", "sqlite");
        };

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that observability forwards traces, timing, and counters when services are registered.
    /// </summary>
    [Fact]
    public void Methods_ShouldForwardSignalsToTracingAndMetrics()
    {
        using var traces = new ActivityRecorder();
        using var metrics = new MetricRecorder();

        var services = new ServiceCollection();
        services.AddSingleton<IMeterFactory>(metrics);

        var observability = CreateObservability(services.BuildServiceProvider());

        using (InvokeBeginOperation(observability, "postgresql", "migrate"))
        {
            InvokeAddTag(observability, "database.configurator.count", "2");
            InvokeRecordSuccess(observability, "postgresql", "migrate");
            InvokeRecordFailure(observability, "postgresql", "migrate");
        }

        traces.Stopped.Should().ContainSingle();
        var activity = traces.Stopped[0];
        activity.OperationName.Should().Be("database.migrate");
        activity.Kind.Should().Be(ActivityKind.Client);
        activity.GetTagItem("raycynix.database.provider").Should().Be("postgresql");
        activity.GetTagItem("raycynix.database.operation").Should().Be("migrate");
        activity.GetTagItem("database.configurator.count").Should().Be("2");
        activity.GetTagItem("raycynix.database.status").Should().Be("failure");
        activity.Status.Should().Be(ActivityStatusCode.Error);

        metrics.DoubleMeasurements.Should().ContainSingle(measurement =>
            measurement.InstrumentName == "raycynix.database.operation.duration" &&
            HasTag(measurement.Tags, "raycynix.database.provider", "postgresql") &&
            HasTag(measurement.Tags, "raycynix.database.operation", "migrate"));
        metrics.LongMeasurements.Should().Contain(measurement =>
            measurement.InstrumentName == "raycynix.database.operations" &&
            HasTag(measurement.Tags, "raycynix.database.status", "success"));
        metrics.LongMeasurements.Should().Contain(measurement =>
            measurement.InstrumentName == "raycynix.database.operations" &&
            HasTag(measurement.Tags, "raycynix.database.status", "failure"));
    }

    private static object CreateObservability(IServiceProvider serviceProvider)
    {
        var type = typeof(global::Raycynix.Extensions.Database.Observability.Observability).Assembly
            .GetType("Raycynix.Extensions.Database.Observability.DatabaseObservability")!;

        return Activator.CreateInstance(type, serviceProvider)!;
    }

    private static IDisposable InvokeBeginOperation(object observability, string providerName, string operation)
    {
        return (IDisposable)observability.GetType()
            .GetMethod("BeginOperation")!
            .Invoke(observability, [providerName, operation])!;
    }

    private static void InvokeRecordSuccess(object observability, string providerName, string operation)
    {
        observability.GetType().GetMethod("RecordSuccess")!.Invoke(observability, [providerName, operation]);
    }

    private static void InvokeRecordFailure(object observability, string providerName, string operation)
    {
        observability.GetType().GetMethod("RecordFailure")!.Invoke(observability, [providerName, operation]);
    }

    private static void InvokeAddTag(object observability, string key, string value)
    {
        observability.GetType().GetMethod("AddTag")!.Invoke(observability, [key, value]);
    }

    private sealed class ActivityRecorder : IDisposable
    {
        private readonly ActivityListener _listener;

        public ActivityRecorder()
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == RaycynixTracing.SourceName,
                Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity => Stopped.Add(activity)
            };
            ActivitySource.AddActivityListener(_listener);
        }

        public List<Activity> Stopped { get; } = [];

        public void Dispose() => _listener.Dispose();
    }

    private static bool HasTag(
        IReadOnlyCollection<KeyValuePair<string, object?>> tags,
        string key,
        object value)
    {
        return tags.Any(tag => tag.Key == key && Equals(tag.Value, value));
    }

    private sealed class MetricRecorder : IMeterFactory, IDisposable
    {
        private readonly MeterListener _listener;
        private readonly List<Meter> _meters = [];

        public MetricRecorder()
        {
            _listener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name == RaycynixMetrics.MeterName)
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                LongMeasurements.Add(new(instrument.Name, value, tags.ToArray())));
            _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                DoubleMeasurements.Add(new(instrument.Name, value, tags.ToArray())));
            _listener.Start();
        }

        public List<Measurement<long>> LongMeasurements { get; } = [];
        public List<Measurement<double>> DoubleMeasurements { get; } = [];

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options);
            _meters.Add(meter);
            return meter;
        }

        public void Dispose()
        {
            _listener.Dispose();
            foreach (var meter in _meters)
            {
                meter.Dispose();
            }
        }
    }

    private sealed record Measurement<T>(
        string InstrumentName,
        T Value,
        KeyValuePair<string, object?>[] Tags);

}
