# Raycynix.Extensions.Database.Abstractions

Contracts and configuration models shared by the Raycynix database packages.

## What It Provides

- `DatabaseConfiguration` and `ConnectionConfiguration`
- `IDatabaseBuilder`
- `IDatabaseInitializer`
- `IDatabaseProviderRegistration`
- `IDatabaseModelAssemblyRegistry`
- `IDatabaseObservability`
- `IConfigurator` and `IGenericConfigurator<T>`
- `DatabaseTableAttribute`

## Provider Contracts

Provider packages implement `IDatabaseProviderRegistration` to validate provider-specific settings, resolve a connection string, and configure EF Core provider options.

```csharp
public interface IDatabaseProviderRegistration
{
    string ProviderName { get; }

    void Validate(DatabaseConfiguration configuration);

    string ResolveConnectionString(
        DatabaseConfiguration configuration,
        IServiceProvider serviceProvider);

    void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider);
}
```

Common validation stays in `DatabaseConfiguration`. Provider-specific rules, such as whether `Host` or `Username` is required, belong in the provider implementation.

## Configurators

Reusable packages can contribute EF Core mappings through configurators:

```csharp
[DatabaseTable("orders")]
public sealed class OrderConfigurator : IGenericConfigurator<Order>
{
    public Type Type => typeof(Order);

    public Type[] DependsOn => [];

    public string ModelCacheKey => typeof(Order).FullName!;

    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();
        entity.ToTable("orders");
        entity.HasKey(static order => order.Id);
    }

    public void Seed(ModelBuilder modelBuilder)
    {
    }
}
```

If a configurator changes the model shape from runtime values, include those values in `ModelCacheKey` so EF Core does not reuse an incompatible cached model.

## Usage

This package is intended for provider packages, optional feature packages, and reusable modules that need database contracts without depending on the core runtime registration package.
