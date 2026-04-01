# Raycynix.Extensions.Database

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database` is the core database package.

## What it contains

- `AddRaycynixDatabase(...)`
- `AddRaycynixDatabaseAssembly(...)`
- `DatabaseBuilder.AddAssembly(...)`
- `DatabaseContext`
- `DatabaseConfiguration`
- provider-specific EF Core setup
- `IDatabaseInitializer`
- `DatabaseInitializer`

## What it does not contain

- `WebApplication` extensions
- ASP.NET Core startup integration
- generic-host startup integration

## Usage

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration, options =>
{
    options.Provider = DatabaseProvider.PostgreSql;
    options.UseMigrations = true;
});
```

If a reusable package contributes EF Core configurators to the shared `DatabaseContext`, register its assembly explicitly:

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration)
    .AddAssembly<SomePackageMarker>();
```

This keeps a single shared `DatabaseContext` while allowing infrastructure packages to extend the model without creating their own context.

For static table names, configurators can declare the default mapping with `DatabaseTableAttribute` instead of calling `ToTable(...)` manually inside `Configure(...)`.

For runtime overrides, `GenericConfigurator<T>` now exposes `ConfigureEntity(modelBuilder, tableName)`, so a configurator can reuse the same default mapping logic while still supplying a table name from configuration.

If you want an explicit fluent call, use `modelBuilder.Entity<T>().EntityName(tableName)`.

If you want to run initialization during startup, use one of these packages:

- `Raycynix.Extensions.Database.Hosting`
- `Raycynix.Extensions.Database.AspNetCore`
