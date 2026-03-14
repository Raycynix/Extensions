using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Provides an abstract base implementation for configuring database entities of a specified type.
/// </summary>
/// <typeparam name="T">
/// The type of the entity to be configured. Must be a reference type.
/// </typeparam>
public abstract class GenericConfigurator<T> : IGenericConfigurator<T> where T : class
{
    /// <summary>
    /// 
    /// </summary>
    public Type Type => typeof(T);

    /// <summary>
    /// Specifies a dependency between this property and one or more other properties.
    /// Changes made to the properties listed in this dependency may trigger actions
    /// or recalculations related to the current property.
    /// </summary>
    /// <remarks>
    /// This property is commonly used in scenarios where the value or behavior of a property
    /// is influenced by changes in other properties, such as in data binding or validation frameworks.
    /// </remarks>
    /// <example>
    /// When a property depends on other properties, the dependent property
    /// can be updated automatically if any of the specified properties change.
    /// </example>
    public abstract Type[] DependsOn { get; }

    /// <summary>
    /// Configures the entity of type <typeparamref name="T"/> within the specified <see cref="ModelBuilder"/>.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to configure the entity.
    /// </param>
    public virtual void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<T>().ToTable(typeof(T).Name);
    }

    /// <summary>
    /// Seeds the database with entities and data using the provided <see cref="ModelBuilder"/>.
    /// This method can be overridden to provide custom seeding logic for the entity type.
    /// </summary>
    /// <param name="modelBuilder">
    /// An instance of <see cref="ModelBuilder"/> used to configure and seed entities in the database.
    /// </param>
    public virtual void Seed(ModelBuilder modelBuilder)
    {
    }
}