# Raycynix.Extensions.Database

Core EF Core database infrastructure for Raycynix applications.

## What It Provides

- `AddRaycynixDatabase(...)` zero-setup registration with the default `DatabaseContext`
- `AddRaycynixDatabase<TContext>(...)` for custom Raycynix database contexts
- marker and explicit assembly overloads for model configurators and EF Core migrations
- model assembly discovery through `AddAssembly(...)`
- `RaycynixDatabaseContext`, `GenericConfigurator<T>`, and table-name helpers
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
  "DatabaseOptions": {
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

Provider-specific settings are nested under `DatabaseOptions`:

```json
{
  "DatabaseOptions": {
    "PostgreSqlOptions": {
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

## Logging

The package uses the standard `Microsoft.Extensions.Logging.ILogger<T>` abstraction when a logger is available. Logger dependencies are optional, so the package can run without registering a logging provider. It does not require `Raycynix.Extensions.Logging`; any Microsoft-compatible logging provider can receive the events.

Database registration, initialization, migrations, creation, and model configuration emit operational diagnostics. Connection strings, usernames, passwords, and provider secrets are never logged.

Enable Debug logs when troubleshooting provider resolution, DbContext setup, model configurators, initialization, or migrations:

```json
{
  "Logging": {
    "LogLevel": {
      "Raycynix.Extensions.Database": "Debug"
    }
  }
}
```

## Custom Contexts

```csharp
public sealed class AppDatabaseContext : DbContext, IRaycynixDatabaseContext
{
    private readonly IDatabaseModelConfigurator _modelConfigurator;
    private readonly string _providerName;

    public AppDatabaseContext(
        DbContextOptions options,
        DatabaseOptions config,
        IDatabaseModelConfigurator modelConfigurator,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _modelConfigurator = modelConfigurator;
        _providerName = serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().ProviderName;

        ChangeTracker.LazyLoadingEnabled = config.EnableLazyLoading;
        ChangeTracker.AutoDetectChangesEnabled = config.EnableAutoDetectChanges;
        ChangeTracker.QueryTrackingBehavior = config.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _modelConfigurator.Configure(builder, _providerName);
    }

    public string GetModelCacheKey()
    {
        return _modelConfigurator.GetModelCacheKey(_providerName);
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

If `AddRaycynixDatabase` is called more than once with the same context, only the first call may configure `DatabaseOptions`. Later calls can add assemblies but cannot pass another `setup` callback.

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

## Migrating From 2.x

- Replace `DatabaseConfiguration` with `DatabaseOptions`.
- Replace `ConnectionConfiguration` and the nested `ConnectionConfiguration` key with `ConnectionOptions`.
- Import shared settings from `Raycynix.Extensions.Database.Abstractions.Options`.
- Rename the root configuration section from `DatabaseConfiguration` to `DatabaseOptions`.
- Rename provider sections to their options type names, for example `PostgreSqlOptions` or `SqliteOptions`.
