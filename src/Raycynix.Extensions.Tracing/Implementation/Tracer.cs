using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.Implementation;

/// <inheritdoc />
public class Tracer : ITracer
{
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes a new tracer for the specified service name.
    /// </summary>
    public Tracer(string serviceName)
    {
        _activitySource = new ActivitySource(serviceName);
    }

    /// <summary>
    /// Starts a trace activity and optionally attaches tags.
    /// </summary>
    /// <param name="name">The name of the activity to start.</param>
    /// <param name="tags">Optional tags to be added as key-value pairs for associating metadata with the activity.</param>
    /// <returns>A handle that ends the activity when disposed.</returns>
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
    /// Adds a tag to the current activity.
    /// </summary>
    /// <param name="key">The key of the tag to add.</param>
    /// <param name="value">The value of the tag associated with the specified key.</param>
    public void AddTag(string key, string value) => Activity.Current?.SetTag(key, value);

    /// <summary>
    /// Sets a baggage value on the current activity.
    /// </summary>
    /// <param name="key">The key of the baggage item to set.</param>
    /// <param name="value">The value of the baggage item to set for the specified key.</param>
    public void SetBaggage(string key, string value) => Activity.Current?.SetBaggage(key, value);

    /// <summary>
    /// Gets a baggage value from the current activity.
    /// </summary>
    /// <param name="key">The key of the baggage item to retrieve.</param>
    /// <returns>The baggage value, or <c>null</c> when it does not exist.</returns>
    public string? GetBaggage(string key) => Activity.Current?.GetBaggageItem(key);

    private class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
