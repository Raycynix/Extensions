# Raycynix.Extensions.Database.AspNetCore

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.AspNetCore` adds ASP.NET Core startup integration.

## What it does

This package exposes `UseRaycynixDatabaseInitializationAsync(this WebApplication app)` and delegates the actual work to `Raycynix.Extensions.Database.Hosting`.

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
