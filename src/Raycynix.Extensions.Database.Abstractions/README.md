# Raycynix.Extensions.Database.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.Abstractions` contains the contracts used by the Raycynix database packages.

## What it contains

- `IDatabaseInitializer`
- `IConfigurator`
- `IGenericConfigurator<T>`
- `DatabaseTableAttribute`

`IConfigurator` describes both model configuration and the cache key fragment that identifies the model shape produced by that configurator. This allows reusable packages to contribute EF Core mappings without breaking shared model caching.

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

The application can then register the assembly containing that configurator through `AddRaycynixDatabase(...).AddAssembly<TMarker>()`.
