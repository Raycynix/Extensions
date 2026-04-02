using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;

namespace Raycynix.Extensions.Database.Internal;

internal sealed class DatabaseObservability
{
    private readonly ITracer? _tracer;
    private readonly IMetricCounter? _operationCounter;
    private readonly IMetricHistogram? _operationDuration;

    public DatabaseObservability(IServiceProvider serviceProvider)
    {
        _tracer = serviceProvider.GetService(typeof(ITracer)) as ITracer;

        var metricsService = serviceProvider.GetService(typeof(IMetricsService)) as IMetricsService;
        if (metricsService is null)
        {
            return;
        }

        _operationCounter = metricsService.CreateCounter(
            "raycynix_database_operations_total",
            "Total number of observed database operations.",
            "provider",
            "operation",
            "status");

        _operationDuration = metricsService.CreateHistogram(
            "raycynix_database_operation_duration_seconds",
            "Duration of observed database operations.",
            "provider",
            "operation");
    }

    public IDisposable BeginOperation(DatabaseProvider provider, string operation)
    {
        var providerName = provider.ToString().ToLowerInvariant();
        var timer = _operationDuration?.MeasureDuration(providerName, operation) ?? NoopDisposable.Instance;
        var trace = _tracer?.StartTrace($"database.{operation}", new Dictionary<string, string>
        {
            ["database.provider"] = providerName,
            ["database.operation"] = operation
        }) ?? NoopDisposable.Instance;

        return new CompositeDisposable(timer, trace);
    }

    public void RecordSuccess(DatabaseProvider provider, string operation)
    {
        Record(provider, operation, "success");
    }

    public void RecordFailure(DatabaseProvider provider, string operation)
    {
        Record(provider, operation, "failure");
    }

    public void AddTag(string key, string value)
    {
        _tracer?.AddTag(key, value);
    }

    private void Record(DatabaseProvider provider, string operation, string status)
    {
        _operationCounter?.Increment(
            labelValues:
            [
                provider.ToString().ToLowerInvariant(),
                operation,
                status
            ]);
    }

    private sealed class CompositeDisposable(IDisposable first, IDisposable second) : IDisposable
    {
        public void Dispose()
        {
            second.Dispose();
            first.Dispose();
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
