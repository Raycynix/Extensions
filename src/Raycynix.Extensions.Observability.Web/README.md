# Raycynix.Extensions.Observability.Web

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Observability.Web` adds ASP.NET Core integration for Raycynix observability.

## What it contains

- `AddRaycynixWebObservability(...)`
- `UseRaycynixObservability(this IApplicationBuilder app)`
- `MapRaycynixObservabilityEndpoints(this IEndpointRouteBuilder endpoints)`
- correlation middleware and `HttpClient` correlation propagation

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixObservability();
builder.Services.AddRaycynixWebObservability();

var app = builder.Build();

app.UseRaycynixObservability();
app.MapRaycynixObservabilityEndpoints();

app.Run();
```
