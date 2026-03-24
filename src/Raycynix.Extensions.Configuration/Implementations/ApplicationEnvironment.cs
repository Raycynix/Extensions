using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Implementations;

internal sealed class ApplicationEnvironment(string name) : IApplicationEnvironment
{
    public string Name { get; } = name;

    public bool IsDevelopment => Is(EnvironmentNames.Development);

    public bool IsTesting => Is(EnvironmentNames.Testing);

    public bool IsStaging => Is(EnvironmentNames.Staging);

    public bool IsProduction => Is(EnvironmentNames.Production);

    private bool Is(string environmentName)
    {
        return string.Equals(Name, environmentName, StringComparison.OrdinalIgnoreCase);
    }
}
