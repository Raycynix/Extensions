namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Provides unified access to a typed configuration model in application code.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public interface IConfigurationAccessor<out TOptions>
    where TOptions : class
{
    /// <summary>
    /// Gets the current configuration snapshot.
    /// </summary>
    TOptions Current { get; }

    /// <summary>
    /// Gets a named configuration snapshot.
    /// </summary>
    /// <param name="name">The options instance name.</param>
    /// <returns>The named configuration snapshot.</returns>
    TOptions Get(string? name);
}
