using Raycynix.Extensions.Common.Disposables;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Database.Observability;

/// <summary>
/// Coordinates optional tracing and metrics emission for database operations.
/// </summary>
internal sealed class DatabaseObservability : IDatabaseObservability
{
    private readonly ITracer? _tracer;
    private readonly IMetricCounter? _operationCounter;
    private readonly IMetricHistogram? _operationDuration;
    private readonly ILogger<DatabaseObservability>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseObservability"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve optional observability services.</param>
    public DatabaseObservability(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService(typeof(ILogger<DatabaseObservability>)) as ILogger<DatabaseObservability>;
        _tracer = serviceProvider.GetService(typeof(ITracer)) as ITracer;

        if (serviceProvider.GetService(typeof(IMetricsService)) is not IMetricsService metricsService)
        {
            _logger?.LogDebug(
                "Database observability initialized without metrics service. Tracing enabled: {TracingEnabled}.",
                _tracer is not null);
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

        _logger?.LogDebug(
            "Database observability initialized. Metrics enabled: {MetricsEnabled}, Tracing enabled: {TracingEnabled}.",
            true,
            _tracer is not null);
    }

    /// <summary>
    /// Starts timing and tracing for a database operation.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    /// <returns>A disposable scope that completes the timing and tracing operation.</returns>
    public IDisposable BeginOperation(string providerName, string operation)
    {
        providerName = providerName.ToLowerInvariant();
        _logger?.LogDebug(
            "Beginning observed database operation {Operation} for provider {ProviderName}.",
            operation,
            providerName);

        var timer = _operationDuration?.MeasureDuration(providerName, operation) ?? NoopDisposable.Instance;
        var trace = _tracer?.StartTrace($"database.{operation}", new Dictionary<string, string>
        {
            ["database.provider"] = providerName,
            ["database.operation"] = operation
        }) ?? NoopDisposable.Instance;

        return new CompositeDisposable(timer, trace);
    }

    /// <summary>
    /// Records a successful database operation result.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    public void RecordSuccess(string providerName, string operation)
    {
        Record(providerName, operation, "success");
    }

    /// <summary>
    /// Records a failed database operation result.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    public void RecordFailure(string providerName, string operation)
    {
        Record(providerName, operation, "failure");
    }

    /// <summary>
    /// Adds a trace tag when tracing is enabled.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    public void AddTag(string key, string value)
    {
        _tracer?.AddTag(key, value);
    }

    private void Record(string providerName, string operation, string status)
    {
        _operationCounter?.Increment(
            labelValues:
            [
                providerName.ToLowerInvariant(),
                operation,
                status
            ]);

        _logger?.LogDebug(
            "Recorded database operation {Operation} for provider {ProviderName} with status {Status}.",
            operation,
            providerName,
            status);
    }

    private sealed class CompositeDisposable(IDisposable first, IDisposable second) : IDisposable
    {
        public void Dispose()
        {
            second.Dispose();
            first.Dispose();
        }
    }

}
