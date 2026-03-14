using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions.Configurators;

/// <summary>
/// Defines the contract for configuring database entities and managing their dependencies.
/// </summary>
public interface IConfigurator
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the database entity being configured.
    /// </summary>
    /// <remarks>
    /// This property represents the specific entity type that the configurator is responsible for.
    /// It is typically used in database configuration or dependency resolution contexts to identify
    /// and work with specific entity types.
    /// </remarks>
    Type Type { get; }

    /// <summary>
    /// Gets the collection of types that the current configuration depends on.
    /// </summary>
    /// <remarks>
    /// This property represents the set of entity types that must be configured prior to the current entity.
    /// It is commonly used to define dependencies between configurations, ensuring that certain entities
    /// are set up in a specific order to maintain database consistency and integrity.
    /// </remarks>
    Type[] DependsOn { get; }

    /// <summary>
    /// Configures the entity model for a specific type within the Entity Framework Core model builder.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to define the entity model configuration.
    /// </param>
    void Configure(ModelBuilder modelBuilder);

    /// <summary>
    /// Seeds the database with initial data for the entity model.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to define the seeding configuration.
    /// </param>
    void Seed(ModelBuilder modelBuilder);
}