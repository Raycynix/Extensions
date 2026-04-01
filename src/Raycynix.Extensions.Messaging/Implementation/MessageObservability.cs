using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Messaging.Implementation;

/// <summary>
/// Records optional metrics for messaging publish and dispatch operations.
/// </summary>
public sealed class MessageObservability
{
    private readonly IMetricCounter? _dispatchCounter;
    private readonly IMetricHistogram? _dispatchDuration;
    private readonly IMetricCounter? _publishCounter;
    private readonly IMetricHistogram? _publishDuration;

    /// <summary>
    /// Initializes a new observability helper instance.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public MessageObservability(IServiceProvider serviceProvider)
    {
        var metricsService = serviceProvider.GetService(typeof(IMetricsService)) as IMetricsService;
        if (metricsService is null)
        {
            return;
        }

        _dispatchCounter = metricsService.CreateCounter(
            "raycynix_messaging_dispatch_total",
            "Total number of observed messaging dispatch operations.",
            "message_type",
            "destination",
            "status");

        _dispatchDuration = metricsService.CreateHistogram(
            "raycynix_messaging_dispatch_duration_seconds",
            "Duration of observed messaging dispatch operations.",
            "message_type",
            "destination");

        _publishCounter = metricsService.CreateCounter(
            "raycynix_messaging_publish_total",
            "Total number of observed messaging publish operations.",
            "format",
            "destination",
            "status");

        _publishDuration = metricsService.CreateHistogram(
            "raycynix_messaging_publish_duration_seconds",
            "Duration of observed messaging publish operations.",
            "format",
            "destination");
    }

    /// <summary>
    /// Starts observing a dispatch operation.
    /// </summary>
    /// <param name="messageType">The message payload type.</param>
    /// <param name="destination">The logical destination.</param>
    /// <returns>A timer handle.</returns>
    public IDisposable BeginDispatch(Type messageType, string destination)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        return _dispatchDuration?.MeasureDuration(GetMessageTypeName(messageType), destination) ?? NoopDisposable.Instance;
    }

    /// <summary>
    /// Records a successful dispatch operation.
    /// </summary>
    /// <param name="messageType">The message payload type.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordDispatchSuccess(Type messageType, string destination)
    {
        RecordDispatch(messageType, destination, "success");
    }

    /// <summary>
    /// Records a failed dispatch operation.
    /// </summary>
    /// <param name="messageType">The message payload type.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordDispatchFailure(Type messageType, string destination)
    {
        RecordDispatch(messageType, destination, "failure");
    }

    /// <summary>
    /// Starts observing a publish operation.
    /// </summary>
    /// <param name="format">The payload format.</param>
    /// <param name="destination">The logical destination.</param>
    /// <returns>A timer handle.</returns>
    public IDisposable BeginPublish(string format, string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        return _publishDuration?.MeasureDuration(format, destination) ?? NoopDisposable.Instance;
    }

    /// <summary>
    /// Records a successful publish operation.
    /// </summary>
    /// <param name="format">The payload format.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordPublishSuccess(string format, string destination)
    {
        RecordPublish(format, destination, "success");
    }

    /// <summary>
    /// Records a failed publish operation.
    /// </summary>
    /// <param name="format">The payload format.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordPublishFailure(string format, string destination)
    {
        RecordPublish(format, destination, "failure");
    }

    private void RecordDispatch(Type messageType, string destination, string status)
    {
        _dispatchCounter?.Increment(labelValues: [GetMessageTypeName(messageType), destination, status]);
    }

    private void RecordPublish(string format, string destination, string status)
    {
        _publishCounter?.Increment(labelValues: [format, destination, status]);
    }

    private static string GetMessageTypeName(Type messageType)
    {
        return messageType.FullName ?? messageType.Name;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
