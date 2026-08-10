using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Infrastructure;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Configurators;

/// <summary>
/// Configures the messaging outbox entity in the shared database context.
/// </summary>
internal sealed class MessagingOutboxEntryConfigurator(
    MessagingDatabasePersistenceConfiguration configuration) : GenericConfigurator<MessagingOutboxEntryEntity>
{
    /// <inheritdoc />
    public override Type[] DependsOn => [];

    /// <inheritdoc />
    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MessagingOutboxEntryEntity>()
            .EntityName(configuration.OutboxTableName);
        
        entity.HasKey(static entry => entry.MessageId);
        entity.Property(static entry => entry.MessageId).IsRequired().HasMaxLength(256);
        
        entity.Property(static entry => entry.Destination).IsRequired();
        
        entity.Property(static entry => entry.Payload).IsRequired();
        
        entity.Property(static entry => entry.ContentType).IsRequired().HasMaxLength(256);
        
        entity.Property(static entry => entry.CreatedAt)
            .HasConversion<UtcDateTimeOffsetTicksConverter>()
            .IsRequired();
        
        entity.Property(static entry => entry.Headers).IsRequired();
        
        entity.Property(static entry => entry.Status).IsRequired();
        
        entity.Property(static entry => entry.UpdatedAt)
            .HasConversion<UtcDateTimeOffsetTicksConverter>()
            .IsRequired()
            .IsConcurrencyToken();
        
        entity.Property(static entry => entry.AttemptCount).IsRequired();
        
        entity.Property(static entry => entry.NextAttemptAt)
            .HasConversion<UtcDateTimeOffsetTicksConverter>()
            .IsRequired();
        
        entity.HasIndex(static entry => new { entry.Status, entry.NextAttemptAt });
        entity.HasIndex(static entry => new { entry.Status, entry.UpdatedAt });
    }

    /// <inheritdoc />
    protected override string? GetModelShapeCacheKey()
    {
        return configuration.OutboxTableName;
    }
}
