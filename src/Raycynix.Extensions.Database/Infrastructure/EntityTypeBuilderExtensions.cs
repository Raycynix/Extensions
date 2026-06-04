using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raycynix.Extensions.Database.Infrastructure;

/// <summary>
/// Provides convenience extensions for EF Core entity builders used by the Raycynix database package.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Applies the specified table name to the current entity builder.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="entityBuilder">The entity builder to configure.</param>
    /// <param name="tableName">The table name to apply.</param>
    /// <returns>The same entity builder instance.</returns>
    public static EntityTypeBuilder<T> EntityName<T>(
        this EntityTypeBuilder<T> entityBuilder,
        string tableName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entityBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        entityBuilder.ToTable(tableName);
        return entityBuilder;
    }
}
