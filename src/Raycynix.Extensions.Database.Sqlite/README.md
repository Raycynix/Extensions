# Raycynix.Extensions.Database.Sqlite

SQLite provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddSqlite(...)`
- `SqliteConfiguration`
- SQLite structured connection-string composition
- SQLite provider-specific validation
- EF Core `UseSqlite(...)` configuration with command timeout and migrations assembly support

The provider is selected by calling `.AddSqlite(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.EnsureCreated = true;
        options.UseMigrations = false;
    })
    .AddSqlite(sqlite =>
    {
        sqlite.CommandTimeoutSeconds = 30;
    });
```

## Configuration

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

When a raw `ConnectionString` is not supplied, structured SQLite configuration requires only `Name`, which becomes the SQLite data source.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings and data source values are never logged.
