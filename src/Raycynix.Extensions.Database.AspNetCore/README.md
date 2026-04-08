# Raycynix.Extensions.Database.AspNetCore

`Raycynix.Extensions.Database.AspNetCore` adds ASP.NET Core startup integration.

## What it does

This package exposes `InitializeRaycynixDatabaseAsync(this WebApplication app)` and delegates the actual work to `Raycynix.Extensions.Database.Hosting`.

Register the shared database services and exactly one provider package before invoking the ASP.NET Core initializer.

## appsettings.json

```json
{
  "DatabaseConfiguration": {
    "ConnectionString": "Host=localhost;Port=5432;Database=app;Username=app;Password=secret",
    "UseMigrations": true,
    "EnsureCreated": false,
    "PostgreSqlConfiguration": {
      "CommandTimeoutSeconds": 30,
      "IncludeErrorDetail": false
    }
  }
}
```

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql();

var app = builder.Build();

await app.InitializeRaycynixDatabaseAsync();

app.Run();
```

## How it works

The web extension is a thin wrapper:

```csharp
await app.Services.InitializeRaycynixDatabaseAsync(cancellationToken);
```
