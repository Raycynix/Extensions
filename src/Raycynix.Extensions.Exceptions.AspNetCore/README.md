# Raycynix.Extensions.Exceptions.AspNetCore

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

Mapped exceptions are written as structured JSON responses. For example, a mapped validation exception produces an HTTP response body shaped by the core exception package and the ASP.NET Core middleware.

This package depends on `Raycynix.Extensions.Exceptions` for the core exception mapping and retry services.
