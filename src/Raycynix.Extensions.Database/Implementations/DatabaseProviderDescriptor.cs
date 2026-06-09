using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Captures the single resolved database provider registration used by the shared database infrastructure.
/// </summary>
public sealed class DatabaseProviderDescriptor
{
    /// <summary>
    /// Gets the normalized logical name of the active provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets the provider-specific registration implementation.
    /// </summary>
    public required IDatabaseProviderRegistration Registration { get; init; }

    /// <summary>
    /// Resolves the single active database provider registration from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing database provider registrations.</param>
    /// <returns>A descriptor for the active database provider.</returns>
    /// <exception cref="NotSupportedException">Thrown when no database provider is registered.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than one database provider is registered.</exception>
    public static DatabaseProviderDescriptor Resolve(IServiceProvider serviceProvider)
    {
        var registrations = serviceProvider.GetServices<IDatabaseProviderRegistration>().ToArray();

        return registrations.Length switch
        {
            1 => new DatabaseProviderDescriptor
            {
                ProviderName = registrations[0].ProviderName,
                Registration = registrations[0]
            },
            0 => throw new NotSupportedException(
                "No database provider is registered. Add exactly one matching provider package, for example AddSqlite(), AddPostgreSql(), AddMsSql(), or AddMySql()."),
            _ => throw new InvalidOperationException(
                $"Multiple database providers are registered ({string.Join(", ", registrations.Select(static registration => registration.ProviderName))}). Register exactly one database provider package.")
        };
    }
}
