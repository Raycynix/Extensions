# Raycynix.Extensions.Database.PostgreSql

PostgreSQL provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddPostgreSql(...)`
- `PostgreSqlConfiguration`
- PostgreSQL structured connection-string composition
- PostgreSQL provider-specific validation
- EF Core `UseNpgsql(...)` configuration with retries, command timeout, pooling, and migrations assembly support

The provider is selected by calling `.AddPostgreSql(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql(postgreSql =>
    {
        postgreSql.IncludeErrorDetail = false;
        postgreSql.CommandTimeoutSeconds = 30;
    });
```

## Configuration

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
    "EnsureCreated": false,
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

When a raw `ConnectionString` is not supplied, structured PostgreSQL configuration requires `Host`, `Name`, and `Username`.
