# Raycynix.Extensions.Database.Hosting

Generic host startup integration for Raycynix database initialization.

## What It Provides

- `InitializeRaycynixDatabaseAsync(this IServiceProvider serviceProvider)`
- `InitializeRaycynixDatabaseAsync(this IHost host)`

The package resolves `IDatabaseInitializer` from a scope and runs the configured creation or migration steps.

## Usage

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql();

var host = builder.Build();

await host.InitializeRaycynixDatabaseAsync();
await host.RunAsync();
```

Register `Raycynix.Extensions.Database`, exactly one provider package, and any required model assemblies before calling the initializer.

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
      "CommandTimeoutSeconds": 30
    }
  }
}
```

## Logging

The hosting initializer emits optional `Microsoft.Extensions.Logging` diagnostics for startup database initialization. It logs start, completion, and failure events, but does not log connection strings or credentials.
