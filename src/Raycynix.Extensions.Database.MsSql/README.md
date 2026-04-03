# Raycynix.Extensions.Database.MsSql

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.MsSql` adds SQL Server support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddMsSql(...)`
- `MsSqlServerConfiguration`
- SQL Server connection-string building
- `UseSqlServer(...)` integration for the shared `DatabaseContext`

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
