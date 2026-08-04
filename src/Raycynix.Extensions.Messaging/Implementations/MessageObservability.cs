using System.Diagnostics.Metrics;
using Raycynix.Extensions.Common.Disposables;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Records optional metrics for messaging publish and dispatch operations.
/// </summary>
public sealed class MessageObservability
{
    private readonly Counter<long>? _dispatchCounter;
    private readonly Histogram<double>? _dispatchDuration;
    private readonly Counter<long>? _publishCounter;
    private readonly Histogram<double>? _publishDuration;
    private readonly Counter<long>? _requestCounter;
    private readonly Histogram<double>? _requestDuration;

    /// <summary>
    /// Initializes a new observability helper instance.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public MessageObservability(IServiceProvider serviceProvider)
    {
        var meterFactory = serviceProvider.GetService(typeof(IMeterFactory)) as IMeterFactory;
        if (meterFactory is null)
        {
            return;
        }

        var meter = RaycynixMetrics.CreateMeter(meterFactory);
        _dispatchCounter = meter.CreateCounter<long>(
            "raycynix.messaging.dispatches",
            unit: "{dispatch}",
            description: "Number of observed messaging dispatch operations.");
        _dispatchDuration = meter.CreateHistogram<double>(
            "raycynix.messaging.dispatch.duration",
            unit: "s",
            description: "Duration of observed messaging dispatch operations.");
        _publishCounter = meter.CreateCounter<long>(
            "raycynix.messaging.publishes",
            unit: "{publish}",
            description: "Number of observed messaging publish operations.");
        _publishDuration = meter.CreateHistogram<double>(
            "raycynix.messaging.publish.duration",
            unit: "s",
            description: "Duration of observed messaging publish operations.");
        _requestCounter = meter.CreateCounter<long>(
            "raycynix.messaging.requests",
            unit: "{request}",
            description: "Number of observed direct request dispatch operations.");
        _requestDuration = meter.CreateHistogram<double>(
            "raycynix.messaging.request.duration",
            unit: "s",
            description: "Duration of observed direct request dispatch operations.");
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

        return _dispatchDuration?.MeasureDuration(
            new("raycynix.messaging.message.type", GetMessageTypeName(messageType)),
            new("raycynix.messaging.destination", destination)) ?? NoopDisposable.Instance;
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

        return _publishDuration?.MeasureDuration(
            new("raycynix.messaging.message.format", format),
            new("raycynix.messaging.destination", destination)) ?? NoopDisposable.Instance;
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

    /// <summary>
    /// Starts observing a direct request dispatch operation.
    /// </summary>
    /// <param name="requestType">The request payload type.</param>
    /// <param name="destination">The logical destination.</param>
    /// <returns>A timer handle.</returns>
    public IDisposable BeginRequest(Type requestType, string destination)
    {
        ArgumentNullException.ThrowIfNull(requestType);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        return _requestDuration?.MeasureDuration(
            new("raycynix.messaging.request.type", GetMessageTypeName(requestType)),
            new("raycynix.messaging.destination", destination)) ?? NoopDisposable.Instance;
    }

    /// <summary>
    /// Records a successful direct request dispatch operation.
    /// </summary>
    /// <param name="requestType">The request payload type.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordRequestSuccess(Type requestType, string destination)
    {
        RecordRequest(requestType, destination, "success");
    }

    /// <summary>
    /// Records a failed direct request dispatch operation.
    /// </summary>
    /// <param name="requestType">The request payload type.</param>
    /// <param name="destination">The logical destination.</param>
    public void RecordRequestFailure(Type requestType, string destination)
    {
        RecordRequest(requestType, destination, "failure");
    }

    private void RecordDispatch(Type messageType, string destination, string status)
    {
        _dispatchCounter?.Add(
            1,
            new("raycynix.messaging.message.type", GetMessageTypeName(messageType)),
            new("raycynix.messaging.destination", destination),
            new("raycynix.messaging.status", status));
    }

    private void RecordPublish(string format, string destination, string status)
    {
        _publishCounter?.Add(
            1,
            new("raycynix.messaging.message.format", format),
            new("raycynix.messaging.destination", destination),
            new("raycynix.messaging.status", status));
    }

    private void RecordRequest(Type requestType, string destination, string status)
    {
        _requestCounter?.Add(
            1,
            new("raycynix.messaging.request.type", GetMessageTypeName(requestType)),
            new("raycynix.messaging.destination", destination),
            new("raycynix.messaging.status", status));
    }

    private static string GetMessageTypeName(Type messageType)
    {
        return messageType.FullName ?? messageType.Name;
    }

}
