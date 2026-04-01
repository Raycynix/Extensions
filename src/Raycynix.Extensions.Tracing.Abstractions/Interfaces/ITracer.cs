namespace Raycynix.Extensions.Tracing.Abstractions.Interfaces;

/// <summary>
/// Defines a small tracing abstraction built on top of diagnostic activities.
/// </summary>
public interface ITracer
{
    /// <summary>
    /// Starts a trace activity.
    /// </summary>
    /// <param name="name">The name of the trace activity to start.</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <returns>A handle that ends the activity when disposed.</returns>
    IDisposable StartTrace(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Adds a tag to the current activity.
    /// </summary>
    /// <param name="key">The key to identify the tag being added.</param>
    /// <param name="value">The value associated with the specified key.</param>
    void AddTag(string key, string value);

    /// <summary>
    /// Sets a baggage value on the current activity.
    /// </summary>
    /// <param name="key">The key to associate with the baggage item.</param>
    /// <param name="value">The value to associate with the key in the baggage.</param>
    void SetBaggage(string key, string value);

    /// <summary>
    /// Gets a baggage value from the current activity.
    /// </summary>
    /// <param name="key">The key of the baggage item to retrieve.</param>
    /// <returns>The value of the baggage item if found; otherwise, null.</returns>
    string? GetBaggage(string key);
}
