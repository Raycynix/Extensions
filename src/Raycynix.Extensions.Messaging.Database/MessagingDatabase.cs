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
    /// <param name="setup">An optional callback for adjusting the persistence configuration.</param>
    /// <returns>The same builder instance.</returns>
    public static MessagingBuilder AddDatabasePersistence(
        this MessagingBuilder builder,
        Action<MessagingDatabasePersistenceConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuration = new MessagingDatabasePersistenceConfiguration();
        setup?.Invoke(configuration);
        configuration.Validate();

        builder.Services.Replace(ServiceDescriptor.Singleton(configuration));
        builder.Services.AddRaycynixDatabaseAssembly(typeof(MessagingInboxEntryEntity).Assembly);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, MessagingDatabasePersistenceInitializationService>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IIncomingMessageInboxStore, DatabaseIncomingMessageInboxStore>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IMessageOutboxStore, DatabaseMessageOutboxStore>());

        return builder;
    }
}
