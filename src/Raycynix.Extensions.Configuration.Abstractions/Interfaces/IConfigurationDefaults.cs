namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Represents a pluggable source of default values for a typed configuration model.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public interface IConfigurationDefaults<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Applies default values to the provided configuration model before bound configuration values override them.
    /// </summary>
    /// <param name="options">The configuration model to initialize.</param>
    void Apply(TOptions options);
}
