using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Captures the single resolved database provider registration used by the shared database infrastructure.
/// </summary>
internal sealed class DatabaseProviderDescriptor
{
    /// <summary>
    /// Gets the normalized logical name of the active provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets the provider-specific registration implementation.
    /// </summary>
    public required IDatabaseProviderRegistration Registration { get; init; }
}
