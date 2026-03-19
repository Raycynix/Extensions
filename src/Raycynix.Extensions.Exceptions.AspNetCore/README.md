# Raycynix.Extensions.Exceptions.AspNetCore

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Exceptions.AspNetCore` adds ASP.NET Core middleware integration for Raycynix exceptions.

## What it contains

- `UseRaycynixExceptions(this IApplicationBuilder app)`
- `RaycynixExceptionMiddleware`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixExceptions();

var app = builder.Build();

app.UseRaycynixExceptions();

app.Run();
```

This package depends on `Raycynix.Extensions.Exceptions` for the core exception mapping and retry services.
