# Raycynix.Extensions.Database.PostgreSql

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.PostgreSql` adds PostgreSQL support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddPostgreSql(...)`
- `PostgreSqlConfiguration`
- PostgreSQL connection-string building
- `UseNpgsql(...)` integration for the shared `DatabaseContext`

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql(postgreSql =>
    {
        postgreSql.IncludeErrorDetail = true;
        postgreSql.CommandTimeoutSeconds = 30;
    });
```

This package keeps the shared `DatabaseContext` from the core package and only adds PostgreSQL-specific registration on top of it.
