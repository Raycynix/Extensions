# Raycynix.Extensions.Database.MsSql

`Raycynix.Extensions.Database.MsSql` adds SQL Server support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddMsSql(...)`
- `MsSqlServerConfiguration`
- SQL Server connection-string building
- `UseSqlServer(...)` integration for the shared `DatabaseContext`

The provider is selected by calling `.AddMsSql(...)`, not by setting a legacy provider enum in configuration.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddMsSql(sqlServer =>
    {
        sqlServer.TrustServerCertificate = true;
        sqlServer.CommandTimeoutSeconds = 30;
    });
```

## appsettings.json

```json
{
  "DatabaseConfiguration": {
    "ConnectionConfiguration": {
      "Host": "localhost",
      "Name": "app",
      "Username": "sa",
      "Password": "secret"
    },
    "UseMigrations": true,
    "MsSqlServerConfiguration": {
      "TrustServerCertificate": true,
      "MultipleActiveResultSets": false,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

This package keeps the shared `DatabaseContext` from the core package and only adds SQL Server-specific registration on top of it.
