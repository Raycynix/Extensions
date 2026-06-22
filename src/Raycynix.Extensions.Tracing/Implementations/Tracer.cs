using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Disposables;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;

namespace Raycynix.Extensions.Tracing.Implementations;

/// <summary>
/// Implements the Raycynix tracer abstraction on top of <see cref="ActivitySource"/>.
/// </summary>
public class Tracer : ITracer
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<Tracer>? _logger;

    /// <summary>
    /// Initializes a new tracer for the specified service name.
    /// </summary>
    public Tracer(string serviceName, ILogger<Tracer>? logger = null)
    {
        _activitySource = new ActivitySource(serviceName);
        _logger = logger;
        _logger?.LogDebug("Created Raycynix tracer. ServiceName:{ServiceName}", serviceName);
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

        if (activity == null)
        {
            _logger?.LogDebug("Trace activity was not created because no listener is active. TraceName:{TraceName}",
                name);
            return NoopDisposable.Instance;
        }

        _logger?.LogDebug(
            "Started trace activity. TraceName:{TraceName} TagCount:{TagCount}",
            name,
            tags?.Count ?? 0);

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
    public void AddTag(string key, string value)
    {
        Activity.Current?.SetTag(key, value);
        _logger?.LogDebug("Added tag to current trace activity. TagKey:{TagKey} HasActivity:{HasActivity}", key,
            Activity.Current is not null);
    }

    /// <summary>
    /// Sets a baggage value on the current activity.
    /// </summary>
    /// <param name="key">The key of the baggage item to set.</param>
    /// <param name="value">The value of the baggage item to set for the specified key.</param>
    public void SetBaggage(string key, string value)
    {
        Activity.Current?.SetBaggage(key, value);
        _logger?.LogDebug(
            "Set baggage on current trace activity. BaggageKey:{BaggageKey} HasActivity:{HasActivity}",
            key,
            Activity.Current is not null);
    }

    /// <summary>
    /// Gets a baggage value from the current activity.
    /// </summary>
    /// <param name="key">The key of the baggage item to retrieve.</param>
    /// <returns>The baggage value, or <c>null</c> when it does not exist.</returns>
    public string? GetBaggage(string key) => Activity.Current?.GetBaggageItem(key);
}