# Raycynix.Extensions.Metrics.Web

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Metrics.Web` adds ASP.NET Core middleware and endpoint integration for Raycynix metrics.

## What it contains

- `UseRaycynixMetrics(this IApplicationBuilder app)`
- `MapRaycynixMetrics(this IEndpointRouteBuilder endpoints)`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixMetrics();

var app = builder.Build();

app.UseRaycynixMetrics();
app.MapRaycynixMetrics();

app.Run();
```
