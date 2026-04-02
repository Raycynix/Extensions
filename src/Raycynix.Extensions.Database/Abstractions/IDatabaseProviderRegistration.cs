using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines provider-specific database registration behavior.
/// </summary>
public interface IDatabaseProviderRegistration
{
    /// <summary>
    /// Gets the provider handled by the registration.
    /// </summary>
    DatabaseProvider Provider { get; }

    /// <summary>
    /// Resolves the final provider-specific connection string.
    /// </summary>
    string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider);

    /// <summary>
    /// Applies provider-specific EF Core options to the shared database context.
    /// </summary>
    void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider);
}
