namespace Raycynix.Extensions.Configuration.Abstractions.Models;

/// <summary>
/// Represents a typed configuration change event.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public sealed class ConfigurationChangeContext<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationChangeContext{TOptions}"/> class.
    /// </summary>
    /// <param name="previous">The previous configuration snapshot.</param>
    /// <param name="current">The current configuration snapshot.</param>
    /// <param name="name">The option instance name, if any.</param>
    public ConfigurationChangeContext(TOptions previous, TOptions current, string? name)
    {
        Previous = previous;
        Current = current;
        Name = name;
        ChangedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the previous configuration snapshot.
    /// </summary>
    public TOptions Previous { get; }

    /// <summary>
    /// Gets the current configuration snapshot.
    /// </summary>
    public TOptions Current { get; }

    /// <summary>
    /// Gets the option instance name, if any.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the UTC timestamp when the change was observed.
    /// </summary>
    public DateTimeOffset ChangedAtUtc { get; }
}
