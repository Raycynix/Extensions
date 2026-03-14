namespace Raycynix.Extensions.Database.Abstractions.Configurators;

/// <summary>
/// Defines an interface for configuring database entities of a specified type.
/// </summary>
/// <typeparam name="T">
/// The type of the entity to be configured. Must be a reference type.
/// </typeparam>
public interface IGenericConfigurator<T> : IConfigurator where T : class { }