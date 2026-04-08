# Raycynix.Extensions.Database.Sqlite

`Raycynix.Extensions.Database.Sqlite` adds SQLite support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddSqlite(...)`
- `SqliteConfiguration`
- SQLite connection-string building
- `UseSqlite(...)` integration for the shared `DatabaseContext`

The provider is selected by calling `.AddSqlite(...)`, not by setting a legacy provider enum in configuration.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.EnsureCreated = true;
    })
    .AddSqlite(sqlite =>
    {
        sqlite.CommandTimeoutSeconds = 30;
    });
```

## appsettings.json

```json
{
  "DatabaseConfiguration": {
    "ConnectionConfiguration": {
      "Name": "app.db"
    },
    "EnsureCreated": true,
    "UseMigrations": false,
    "SqliteConfiguration": {
      "Mode": "ReadWriteCreate",
      "Cache": "Shared",
      "CommandTimeoutSeconds": 30
    }
  }
}
```

This package keeps the shared `DatabaseContext` from the core package and only adds SQLite-specific registration on top of it.
