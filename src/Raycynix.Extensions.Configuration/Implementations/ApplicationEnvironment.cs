using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Implementations;

/// <summary>
/// Implements the standard application environment abstraction.
/// </summary>
internal sealed class ApplicationEnvironment(string name) : IApplicationEnvironment
{
    /// <inheritdoc />
    public string Name { get; } = name;

    /// <inheritdoc />
    public bool IsDevelopment => Is(EnvironmentNames.Development);

    /// <inheritdoc />
    public bool IsTesting => Is(EnvironmentNames.Testing);

    /// <inheritdoc />
    public bool IsStaging => Is(EnvironmentNames.Staging);

    /// <inheritdoc />
    public bool IsProduction => Is(EnvironmentNames.Production);

    private bool Is(string environmentName)
    {
        return string.Equals(Name, environmentName, StringComparison.OrdinalIgnoreCase);
    }
}
