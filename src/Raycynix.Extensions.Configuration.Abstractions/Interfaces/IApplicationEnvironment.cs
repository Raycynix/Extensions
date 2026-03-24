namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Represents the current application environment in a host-agnostic way.
/// </summary>
public interface IApplicationEnvironment
{
    /// <summary>
    /// Gets the current environment name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the current environment is <c>Development</c>.
    /// </summary>
    bool IsDevelopment { get; }

    /// <summary>
    /// Gets a value indicating whether the current environment is <c>Testing</c>.
    /// </summary>
    bool IsTesting { get; }

    /// <summary>
    /// Gets a value indicating whether the current environment is <c>Staging</c>.
    /// </summary>
    bool IsStaging { get; }

    /// <summary>
    /// Gets a value indicating whether the current environment is <c>Production</c>.
    /// </summary>
    bool IsProduction { get; }
}
