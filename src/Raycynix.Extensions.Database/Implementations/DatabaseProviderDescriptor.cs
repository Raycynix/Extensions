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
