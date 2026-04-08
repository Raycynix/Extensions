# Raycynix.Extensions.Database.Abstractions

`Raycynix.Extensions.Database.Abstractions` contains the contracts used by the Raycynix database packages.

## What it contains

- `IDatabaseInitializer`
- `IConfigurator`
- `IGenericConfigurator<T>`
- `DatabaseTableAttribute`

`IConfigurator` describes both model configuration and the cache key fragment that identifies the model shape produced by that configurator. This allows reusable packages to contribute EF Core mappings without breaking shared model caching.

When a configurator changes the EF Core model shape dynamically, its `ModelCacheKey` must change as well. Static mappings can keep a stable key, while runtime-dependent mappings should include the runtime discriminator, such as a configured table name.

## Purpose

This package exists so database-related contracts can be shared without depending on the full database implementation package.

## Example

Reusable packages can define configurators without referencing the runtime registration package:

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
        entity.HasKey(static current => current.Id);
    }

    public void Seed(ModelBuilder modelBuilder)
    {
    }
}
```

For runtime-dependent mappings, include the runtime value in `ModelCacheKey`:

```csharp
public sealed class OrderConfigurator : IGenericConfigurator<Order>
{
    private readonly OrdersDatabaseOptions options;

    public OrderConfigurator(OrdersDatabaseOptions options)
    {
        this.options = options;
    }

    public Type Type => typeof(Order);

    public Type[] DependsOn => [];

    public string ModelCacheKey => $"{typeof(Order).FullName!}:{options.TableName}";

    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();
        entity.ToTable(options.TableName);
        entity.HasKey(static current => current.Id);
    }

    public void Seed(ModelBuilder modelBuilder)
    {
    }
}
```

The application can then register the assembly containing that configurator through `AddRaycynixDatabase(...).AddAssembly<TMarker>()`.
