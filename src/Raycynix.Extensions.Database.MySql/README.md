# Raycynix.Extensions.Database.MySql

MySQL provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddMySql(...)`
- `MySqlConfiguration`
- MySQL structured connection-string composition
- MySQL provider-specific validation
- EF Core `UseMySQL(...)` configuration with retries, command timeout, pooling, and migrations assembly support

The provider is selected by calling `.AddMySql(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddMySql(mySql =>
    {
        mySql.Pooling = true;
        mySql.CommandTimeoutSeconds = 30;
    });
```

## Configuration

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
    "EnsureCreated": false,
    "MySqlConfiguration": {
      "AllowUserVariables": true,
      "Pooling": true,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured MySQL configuration requires `Host` and `Name`.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings, usernames, and passwords are never logged.
