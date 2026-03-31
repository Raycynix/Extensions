using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Enums;
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
            using var operation = InvokeBeginOperation(observability, DatabaseProvider.Sqlite, "initialization");
            InvokeRecordSuccess(observability, DatabaseProvider.Sqlite, "initialization");
            InvokeRecordFailure(observability, DatabaseProvider.Sqlite, "initialization");
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
        var tracer = new FakeTracer();
        var counter = new FakeMetricCounter();
        var histogram = new FakeMetricHistogram();
        var metricsService = new FakeMetricsService(counter, histogram);

        var services = new ServiceCollection();
        services.AddSingleton<ITracer>(tracer);
        services.AddSingleton<IMetricsService>(metricsService);

        var observability = CreateObservability(services.BuildServiceProvider());

        using (InvokeBeginOperation(observability, DatabaseProvider.PostgreSql, "migrate"))
        {
        }

        InvokeRecordSuccess(observability, DatabaseProvider.PostgreSql, "migrate");
        InvokeRecordFailure(observability, DatabaseProvider.PostgreSql, "migrate");
        InvokeAddTag(observability, "database.configurator.count", "2");

        tracer.StartedTraces.Should().ContainSingle();
        tracer.StartedTraces[0].Name.Should().Be("database.migrate");
        tracer.StartedTraces[0].Tags.Should().Contain(new KeyValuePair<string, string>("database.provider", "postgresql"));
        tracer.StartedTraces[0].Tags.Should().Contain(new KeyValuePair<string, string>("database.operation", "migrate"));
        tracer.Tags.Should().Contain(new KeyValuePair<string, string>("database.configurator.count", "2"));

        var durationLabels = new[] { "postgresql", "migrate" };
        var successLabels = new[] { "postgresql", "migrate", "success" };
        var failureLabels = new[] { "postgresql", "migrate", "failure" };

        histogram.Measurements.Should().ContainSingle(labels => labels.AsEnumerable().SequenceEqual(durationLabels));
        counter.Increments.Should().Contain(labels => labels.AsEnumerable().SequenceEqual(successLabels));
        counter.Increments.Should().Contain(labels => labels.AsEnumerable().SequenceEqual(failureLabels));
    }

    private static object CreateObservability(IServiceProvider serviceProvider)
    {
        var type = typeof(Database).Assembly
            .GetType("Raycynix.Extensions.Database.Internal.DatabaseObservability")!;

        return Activator.CreateInstance(type, serviceProvider)!;
    }

    private static IDisposable InvokeBeginOperation(object observability, DatabaseProvider provider, string operation)
    {
        return (IDisposable)observability.GetType()
            .GetMethod("BeginOperation")!
            .Invoke(observability, [provider, operation])!;
    }

    private static void InvokeRecordSuccess(object observability, DatabaseProvider provider, string operation)
    {
        observability.GetType().GetMethod("RecordSuccess")!.Invoke(observability, [provider, operation]);
    }

    private static void InvokeRecordFailure(object observability, DatabaseProvider provider, string operation)
    {
        observability.GetType().GetMethod("RecordFailure")!.Invoke(observability, [provider, operation]);
    }

    private static void InvokeAddTag(object observability, string key, string value)
    {
        observability.GetType().GetMethod("AddTag")!.Invoke(observability, [key, value]);
    }

    private sealed class FakeTracer : ITracer
    {
        public List<(string Name, Dictionary<string, string> Tags)> StartedTraces { get; } = [];

        public List<KeyValuePair<string, string>> Tags { get; } = [];

        public IDisposable StartTrace(string name, Dictionary<string, string>? tags = null)
        {
            StartedTraces.Add((name, tags ?? []));
            return new FakeDisposable();
        }

        public void AddTag(string key, string value)
        {
            Tags.Add(new KeyValuePair<string, string>(key, value));
        }

        public void SetBaggage(string key, string value)
        {
        }

        public string? GetBaggage(string key)
        {
            return null;
        }
    }

    private sealed class FakeMetricsService(FakeMetricCounter counter, FakeMetricHistogram histogram) : IMetricsService
    {
        public IMetricCounter CreateCounter(string name, string help, params string[] labelNames)
        {
            return counter;
        }

        public IMetricGauge CreateGauge(string name, string help, params string[] labelNames)
        {
            throw new NotSupportedException();
        }

        public IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames)
        {
            return histogram;
        }
    }

    private sealed class FakeMetricCounter : IMetricCounter
    {
        public List<string[]> Increments { get; } = [];

        public void Increment(double value = 1, params string[] labelValues)
        {
            Increments.Add(labelValues);
        }
    }

    private sealed class FakeMetricHistogram : IMetricHistogram
    {
        public List<string[]> Measurements { get; } = [];

        public void Observe(double value, params string[] labelValues)
        {
            Measurements.Add(labelValues);
        }

        public IDisposable MeasureDuration(params string[] labelValues)
        {
            Measurements.Add(labelValues);
            return new FakeDisposable();
        }
    }

    private sealed class FakeDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
