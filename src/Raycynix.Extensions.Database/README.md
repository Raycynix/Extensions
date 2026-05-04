# Raycynix.Extensions.Database

`Raycynix.Extensions.Database` is the core database package.

## What it contains

- `AddRaycynixDatabase(...)`
- `AddRaycynixDatabase<TContext>(...)`
- `AddRaycynixDatabaseAssembly(...)`
- `DatabaseBuilder.AddAssembly(...)`
- `RaycynixDatabaseContext`
- `DatabaseContext`
- provider registration infrastructure
- `IDatabaseInitializer`
- `DatabaseInitializer`
- default no-op database observability

Shared configuration and contracts such as `DatabaseConfiguration`, `IDatabaseProviderRegistration`, and `IDatabaseObservability` live in `Raycynix.Extensions.Database.Abstractions`.

## What it does not contain

- PostgreSQL provider integration
- SQL Server provider integration
- MySQL provider integration
- SQLite provider integration
- `WebApplication` extensions
- ASP.NET Core startup integration
- generic-host startup integration
- tracing and metrics database observability

## Usage

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration, options =>
{
    options.UseMigrations = true;
});
```

## appsettings.json

Core settings stay under `DatabaseConfiguration`:

```json
{
  "DatabaseConfiguration": {
    "ConnectionString": "Host=localhost;Port=5432;Database=app;Username=app;Password=secret",
    "UseMigrations": true,
    "EnsureCreated": false,
    "EnableSeed": true,
    "EnableLazyLoading": false,
    "EnableAutoDetectChanges": true,
    "UseQueryTrackingByDefault": true,
    "RetryCount": 5,
    "RetryDelaySeconds": 10
  }
}
```

The default registration uses `DatabaseContext`:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql();
```

Use `AddRaycynixDatabase<TContext>(...)` when an application needs a custom context while keeping the Raycynix database infrastructure:

```csharp
public sealed class AppDatabaseContext : RaycynixDatabaseContext
{
    public AppDatabaseContext(
        DbContextOptions<AppDatabaseContext> options,
        DatabaseConfiguration config,
        IDatabaseModelAssemblyRegistry modelAssemblyRegistry,
        IDatabaseObservability observability,
        ILogger<RaycynixDatabaseContext> logger,
        IServiceProvider serviceProvider)
        : base(options, config, modelAssemblyRegistry, observability, logger, serviceProvider)
    {
    }
}

builder.Services
    .AddRaycynixDatabase<AppDatabaseContext>(builder.Configuration)
    .AddPostgreSql();
```

Then add exactly one provider package and extend the registration:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql();
```

Equivalent provider packages expose:

- `AddPostgreSql()`
- `AddMsSql()`
- `AddMySql()`
- `AddSqlite()`

The provider package is selected by registration, not by a legacy `Provider` configuration key.

Provider-specific settings remain nested under the same root section:

```json
{
  "DatabaseConfiguration": {
    "PostgreSqlConfiguration": {
      "Pooling": true,
      "MinimumPoolSize": 5,
      "MaximumPoolSize": 50,
      "CommandTimeoutSeconds": 30,
      "IncludeErrorDetail": false
    }
  }
}
```

If a reusable package contributes EF Core configurators to the shared `DatabaseContext`, register its assembly explicitly:

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration)
    .AddAssembly<SomePackageMarker>();
```

This keeps a single shared `DatabaseContext` while allowing infrastructure packages to extend the model without creating their own context.

By default, `AddRaycynixDatabase(...)` also registers the caller assembly for configurator discovery. This is convenient when the application's own configurators live next to the startup code.

If configurator assemblies should be controlled explicitly, disable caller-assembly registration and add the required assemblies yourself:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, registerCallerAssembly: false)
    .AddPostgreSql()
    .AddAssembly<IdentityModelMarker>()
    .AddAssembly<AuditModelMarker>();
```

Use explicit assembly registration in tests, plugin-style architectures, or shared assemblies that contain configurators with optional dependencies.

Provider-specific packages extend the same fluent builder, and the core package expects exactly one database provider registration.

Table names can be configured in three ways, in this order:

1. an explicit runtime name passed to `ConfigureEntity(modelBuilder, tableName)`
2. `DatabaseTableAttribute` on the configurator
3. the entity type name

For static table names, configurators can declare the default mapping with `DatabaseTableAttribute` instead of calling `ToTable(...)` manually inside `Configure(...)`.

For runtime overrides, prefer injecting configuration into the configurator and passing the resolved name to `ConfigureEntity(modelBuilder, tableName)`.

When a configurator changes the model shape at runtime, override `GetModelShapeCacheKey()` so EF Core does not reuse an incompatible cached model.

```csharp
public sealed class OrdersDatabaseOptions
{
    public string TableName { get; init; } = "orders";
}

public sealed class OrderConfigurator(
    OrdersDatabaseOptions options) : GenericConfigurator<Order>
{
    public override Type[] DependsOn => [];

    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = ConfigureEntity(modelBuilder, options.TableName);
        entity.HasKey(static current => current.Id);
    }

    protected override string? GetModelShapeCacheKey()
    {
        return options.TableName;
    }
}
```

If you want an explicit fluent call, use `modelBuilder.Entity<T>().EntityName(tableName)`.

If a configurator relies on runtime-dependent mappings such as table names, schemas, or provider-conditioned model shape, keep those values stable within a given service provider and include them in `GetModelShapeCacheKey()`.

If you want to run initialization during startup, use one of these packages:

- `Raycynix.Extensions.Database.Hosting`
- `Raycynix.Extensions.Database.AspNetCore`

If you want database infrastructure tracing and metrics, add `Raycynix.Extensions.Database.Observability`:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddObservability();
```
