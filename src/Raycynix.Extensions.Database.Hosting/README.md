# Raycynix.Extensions.Database.Hosting

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.Hosting` adds startup integration for generic-host based applications.

## What it does

This package resolves `IDatabaseInitializer` from DI and runs it during application startup.

Register the shared database services and exactly one provider package before invoking the host initializer.

## Available APIs

- `InitializeRaycynixDatabaseAsync(this IServiceProvider serviceProvider)`
- `InitializeRaycynixDatabaseAsync(this IHost host)`

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
      "CommandTimeoutSeconds": 30
    }
  }
}
```

## Usage

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql();

var host = builder.Build();

await host.InitializeRaycynixDatabaseAsync();
await host.RunAsync();
```

## How it works

1. Creates a scope from the service provider
2. Resolves `IDatabaseInitializer`
3. Calls `InitializeAsync(cancellationToken)`
