namespace Raycynix.Extensions.Tracing.Abstractions;

/// <summary>
/// Represents an interface for implementing tracing functionality in an application.
/// </summary>
public interface ITracer
{
    /// <summary>
    /// Starts a trace activity with the specified name and optional tags.
    /// This method initializes and returns a disposable activity, which is used
    /// to track a specific operation or transaction within the system.
    /// </summary>
    /// <param name="name">The name of the trace activity to start.</param>
    /// <param name="tags">An optional dictionary of tags to associate with the trace activity for additional context.</param>
    /// <returns>An <see cref="IDisposable"/> representing the trace activity that was started. Ensure to dispose of it after use to properly end the activity.</returns>
    IDisposable StartTrace(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Adds a tag with the specified key and value to the current context. Tags can be used to
    /// annotate telemetry data with additional metadata for analysis and reporting.
    /// </summary>
    /// <param name="key">The key to identify the tag being added.</param>
    /// <param name="value">The value associated with the specified key.</param>
    void AddTag(string key, string value);

    /// <summary>
    /// Sets a baggage key-value pair in the current activity context. Baggage items are useful for
    /// propagating contextual information across service boundaries.
    /// </summary>
    /// <param name="key">The key to associate with the baggage item.</param>
    /// <param name="value">The value to associate with the key in the baggage.</param>
    void SetBaggage(string key, string value);

    /// <summary>
    /// Retrieves the value of the specified baggage item associated with the current tracing context.
    /// </summary>
    /// <param name="key">The key of the baggage item to retrieve.</param>
    /// <returns>The value of the baggage item if found; otherwise, null.</returns>
    string? GetBaggage(string key);
}