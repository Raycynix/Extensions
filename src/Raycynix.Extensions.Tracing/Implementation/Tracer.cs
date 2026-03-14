using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.Implementation;

/// <inheritdoc />
public class Tracer : ITracer
{
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Provides tracing functionality by leveraging activity-based distributed tracing.
    /// This service aids in tracing operations and propagating context information
    /// across service boundaries within a distributed system.
    /// </summary>
    public Tracer(string serviceName)
    {
        _activitySource = new ActivitySource(serviceName);
    }

    /// <summary>
    /// Starts a trace activity, allowing the creation of a new activity scope
    /// to track operations and associated data within a distributed tracing context.
    /// Optionally, tags can be added to the activity for metadata enrichment.
    /// </summary>
    /// <param name="name">The name of the activity to start.</param>
    /// <param name="tags">Optional tags to be added as key-value pairs for associating metadata with the activity.</param>
    /// <returns>An <see cref="IDisposable"/> instance that manages the lifecycle of the activity. It can be disposed to stop the activity.</returns>
    public IDisposable StartTrace(string name, Dictionary<string, string>? tags = null)
    {
        var activity = _activitySource.StartActivity(name);

        if (activity == null) return new NoopDisposable();
        if (tags == null) return activity;
        
        foreach (var tag in tags)
            activity.SetTag(tag.Key, tag.Value);

        return activity;
    }

    /// <summary>
    /// Adds a tag to the current activity context to provide additional metadata
    /// that can help in tracing or logging operations.
    /// </summary>
    /// <param name="key">The key of the tag to add.</param>
    /// <param name="value">The value of the tag associated with the specified key.</param>
    public void AddTag(string key, string value) => Activity.Current?.SetTag(key, value);

    /// <summary>
    /// Sets a baggage item in the current activity context, allowing the storage
    /// of metadata that can flow through distributed trace contexts.
    /// </summary>
    /// <param name="key">The key of the baggage item to set.</param>
    /// <param name="value">The value of the baggage item to set for the specified key.</param>
    public void SetBaggage(string key, string value) => Activity.Current?.SetBaggage(key, value);

    /// <summary>
    /// Retrieves the baggage value associated with the specified key from the current activity context.
    /// </summary>
    /// <param name="key">The key of the baggage item to retrieve.</param>
    /// <returns>
    /// The value of the baggage item if it exists; otherwise, null.
    /// </returns>
    public string? GetBaggage(string key) => Activity.Current?.GetBaggageItem(key);

    private class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}