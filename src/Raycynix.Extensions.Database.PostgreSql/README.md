# Raycynix.Extensions.Database.PostgreSql

`Raycynix.Extensions.Database.PostgreSql` adds PostgreSQL support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddPostgreSql(...)`
- `PostgreSqlConfiguration`
- PostgreSQL connection-string building
- `UseNpgsql(...)` integration for the shared `DatabaseContext`

The provider is selected by calling `.AddPostgreSql(...)`, not by setting a legacy provider enum in configuration.

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

## appsettings.json

```json
{
  "DatabaseConfiguration": {
    "ConnectionConfiguration": {
      "Host": "localhost",
      "Port": 5432,
      "Name": "app",
      "Username": "app",
      "Password": "secret"
    },
    "UseMigrations": true,
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

This package keeps the shared `DatabaseContext` from the core package and only adds PostgreSQL-specific registration on top of it.
