# Raycynix.Extensions.Database

Core EF Core database infrastructure for Raycynix applications.

## What It Provides

- `AddRaycynixDatabase(...)` zero-setup registration with the default `DatabaseContext`
- `AddRaycynixDatabase<TContext>(...)` for custom Raycynix database contexts
- marker and explicit assembly overloads for model configurators and EF Core migrations
- model assembly discovery through `AddAssembly(...)`
- `RaycynixDatabaseContext`, `DatabaseContext`, `GenericConfigurator<T>`, and table-name helpers
- provider selection through provider packages such as PostgreSQL, SQL Server, MySQL, or SQLite
- startup initialization through `IDatabaseInitializer`
- default no-op database observability

Shared contracts and configuration models live in `Raycynix.Extensions.Database.Abstractions`.

## Basic Usage

For simple applications, call `AddRaycynixDatabase(...)` and one provider. The application entry assembly is registered automatically for configurator discovery and migrations.

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql();
```

Exactly one provider must be registered:

- `AddPostgreSql()`
- `AddMsSql()`
- `AddMySql()`
- `AddSqlite()`

## Configuration

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

Provider-specific settings are nested under `DatabaseConfiguration`:

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

Provider packages validate their own structured connection requirements before building connection strings.

## Custom Contexts

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

## Assembly Registration

The default registration scans the application entry assembly. For modular applications, register model assemblies explicitly:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, registerCallerAssembly: false)
    .AddAssembly<IdentityDatabaseMarker>()
    .AddAssembly<AuditDatabaseMarker>()
    .AddPostgreSql();
```

When model configurators and migrations live in different assemblies, use marker overloads:

```csharp
builder.Services
    .AddRaycynixDatabase<DatabaseContext, AppModelMarker, AppMigrationsMarker>(builder.Configuration)
    .AddPostgreSql();
```

or explicit assemblies:

```csharp
builder.Services
    .AddRaycynixDatabase<DatabaseContext>(
        builder.Configuration,
        migrationsAssembly: typeof(AppMigrationsMarker).Assembly,
        modelAssembly: typeof(AppModelMarker).Assembly)
    .AddPostgreSql();
```

If `AddRaycynixDatabase` is called more than once with the same context, only the first call may configure `DatabaseConfiguration`. Later calls can add assemblies but cannot pass another `setup` callback.

## Configurators

Configurators contribute EF Core model configuration to the shared context:

```csharp
[DatabaseTable("orders")]
public sealed class OrderConfigurator : GenericConfigurator<Order>
{
    public override Type[] DependsOn => [];

    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = ConfigureEntity(modelBuilder);
        entity.HasKey(static order => order.Id);
    }
}
```

Table names are resolved in this order:

1. explicit runtime name passed to `ConfigureEntity(modelBuilder, tableName)`
2. `DatabaseTableAttribute` on the configurator
3. entity type name

If runtime values change the model shape, override `GetModelShapeCacheKey()`:

```csharp
protected override string? GetModelShapeCacheKey()
{
    return options.TableName;
}
```

## Startup Initialization

Use these packages to run initialization during startup:

- `Raycynix.Extensions.Database.Hosting`
- `Raycynix.Extensions.Database.AspNetCore`

## Observability

Database tracing and metrics live in `Raycynix.Extensions.Database.Observability`:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddObservability();
```
