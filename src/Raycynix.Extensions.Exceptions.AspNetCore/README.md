# Raycynix.Extensions.Exceptions.AspNetCore

`Raycynix.Extensions.Exceptions.AspNetCore` adds ASP.NET Core middleware integration for Raycynix exceptions.

## What it contains

- `UseRaycynixExceptions(this IApplicationBuilder app)`
- `RaycynixExceptionMiddleware`
- optional Microsoft `ILogger<T>` diagnostics for handled request failures

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixExceptions();

var app = builder.Build();

app.UseRaycynixExceptions();

app.Run();
```

Mapped exceptions are written as structured JSON responses. For example, a mapped validation exception produces an HTTP response body shaped by the core exception package and the ASP.NET Core middleware.

This package depends on `Raycynix.Extensions.Exceptions` for the core exception mapping and retry services.

## Logging

The middleware uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics include exception category, error code, trace identifiers, method, path, endpoint, whether a query string was present, and masked secure details. Raw query string values are not logged.
