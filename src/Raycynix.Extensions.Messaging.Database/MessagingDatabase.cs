using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Implementations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database;

/// <summary>
/// Provides service registration extensions for database-backed Raycynix messaging persistence.
/// </summary>
public static class MessagingDatabase
{
    /// <summary>
    /// Replaces the default in-memory messaging inbox and outbox stores with database-backed implementations.
    /// </summary>
    /// <param name="builder">The messaging builder to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="sectionName">An optional configuration section name. Defaults to <c>MessagingDatabasePersistenceConfiguration</c>.</param>
    /// <param name="setup">An optional callback for adjusting the persistence configuration.</param>
    /// <returns>The same builder instance.</returns>
    public static MessagingBuilder AddDatabasePersistence(
        this MessagingBuilder builder,
        IConfiguration configuration,
        string? sectionName = null,
        Action<MessagingDatabasePersistenceConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new MessagingDatabasePersistenceConfiguration();
        configuration.GetSection(sectionName ?? nameof(MessagingDatabasePersistenceConfiguration)).Bind(options);
        setup?.Invoke(options);

        return builder.AddDatabasePersistence(options);
    }

    /// <summary>
    /// Replaces the default in-memory messaging inbox and outbox stores with database-backed implementations.
    /// </summary>
    /// <param name="builder">The messaging builder to update.</param>
    /// <param name="setup">An optional callback for adjusting the persistence configuration.</param>
    /// <returns>The same builder instance.</returns>
    public static MessagingBuilder AddDatabasePersistence(
        this MessagingBuilder builder,
        Action<MessagingDatabasePersistenceConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuration = new MessagingDatabasePersistenceConfiguration();
        setup?.Invoke(configuration);
        return builder.AddDatabasePersistence(configuration);
    }

    private static MessagingBuilder AddDatabasePersistence(
        this MessagingBuilder builder,
        MessagingDatabasePersistenceConfiguration configuration)
    {
        configuration.Validate();

        builder.Services.Replace(ServiceDescriptor.Singleton(configuration));
        builder.Services.AddRaycynixDatabaseAssembly(typeof(MessagingInboxEntryEntity).Assembly);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, MessagingDatabasePersistenceInitializationService>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, MessagingDatabaseCleanupService>());
        builder.Services.AddScoped<MessagingDatabaseCleanupProcessor>();
        builder.Services.Replace(ServiceDescriptor.Scoped<IIncomingMessageInboxStore, DatabaseIncomingMessageInboxStore>());
        builder.Services.Replace(ServiceDescriptor.Scoped<IMessageOutboxStore, DatabaseMessageOutboxStore>());

        return builder;
    }
}
