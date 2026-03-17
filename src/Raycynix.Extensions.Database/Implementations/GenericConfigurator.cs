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
    /// Gets the entity type handled by the current configurator.
    /// </summary>
    public Type Type => typeof(T);

    /// <summary>
    /// Gets the entity types whose configurators must be applied before the current one.
    /// </summary>
    public abstract Type[] DependsOn { get; }

    /// <summary>
    /// Applies the default model configuration for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> used to configure the entity mapping.
    /// </param>
    public virtual void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<T>().ToTable(typeof(T).Name);
    }

    /// <summary>
    /// Seeds data for <typeparamref name="T"/> during model creation.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> used to register seed data.
    /// </param>
    public virtual void Seed(ModelBuilder modelBuilder)
    {
    }
}
