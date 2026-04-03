# Raycynix.Extensions.Database

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database` is the core database package.

## What it contains

- `AddRaycynixDatabase(...)`
- `AddRaycynixDatabaseAssembly(...)`
- `DatabaseBuilder.AddAssembly(...)`
- `DatabaseContext`
- `DatabaseConfiguration`
- provider registration infrastructure
- `IDatabaseInitializer`
- `DatabaseInitializer`

## What it does not contain

- PostgreSQL provider integration
- SQL Server provider integration
- MySQL provider integration
- SQLite provider integration
- `WebApplication` extensions
- ASP.NET Core startup integration
- generic-host startup integration

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

Provider-specific packages extend the same fluent builder and the core package expects exactly one database provider registration.

For static table names, configurators can declare the default mapping with `DatabaseTableAttribute` instead of calling `ToTable(...)` manually inside `Configure(...)`.

For runtime overrides, `GenericConfigurator<T>` now exposes `ConfigureEntity(modelBuilder, tableName)`, so a configurator can reuse the same default mapping logic while still supplying a table name from configuration.

If you want an explicit fluent call, use `modelBuilder.Entity<T>().EntityName(tableName)`.

If you want to run initialization during startup, use one of these packages:

- `Raycynix.Extensions.Database.Hosting`
- `Raycynix.Extensions.Database.AspNetCore`
