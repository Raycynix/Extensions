# Raycynix.Extensions.Database.MsSql

SQL Server provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddMsSql(...)`
- `MsSqlServerConfiguration`
- SQL Server structured connection-string composition
- SQL Server provider-specific validation
- EF Core `UseSqlServer(...)` configuration with retries, command timeout, and migrations assembly support

The provider is selected by calling `.AddMsSql(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddMsSql(sqlServer =>
    {
        sqlServer.TrustServerCertificate = false;
        sqlServer.CommandTimeoutSeconds = 30;
    });
```

## Configuration

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
    "EnsureCreated": false,
    "MsSqlServerConfiguration": {
      "TrustServerCertificate": false,
      "MultipleActiveResultSets": false,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured SQL Server configuration requires `Host` and `Name`. Username and password are passed through when provided.
