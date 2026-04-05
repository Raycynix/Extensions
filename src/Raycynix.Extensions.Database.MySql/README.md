# Raycynix.Extensions.Database.MySql

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.MySql` adds MySQL support to `Raycynix.Extensions.Database`.

## What it contains

- `DatabaseBuilder.AddMySql(...)`
- `MySqlConfiguration`
- MySQL connection-string building
- `UseMySql(...)` integration for the shared `DatabaseContext`

The provider is selected by calling `.AddMySql(...)`, not by setting a legacy provider enum in configuration.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddMySql(mySql =>
    {
        mySql.Pooling = true;
        mySql.CommandTimeoutSeconds = 30;
    });
```

## appsettings.json

```json
{
  "DatabaseConfiguration": {
    "ConnectionConfiguration": {
      "Host": "localhost",
      "Port": 3306,
      "Name": "app",
      "Username": "app",
      "Password": "secret"
    },
    "UseMigrations": true,
    "MySqlConfiguration": {
      "AllowUserVariables": true,
      "Pooling": true,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

This package keeps the shared `DatabaseContext` from the core package and only adds MySQL-specific registration on top of it.
