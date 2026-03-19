# Raycynix.Extensions.Observability

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Observability` contains the core observability composition for Raycynix applications.

## What it contains

- `AddRaycynixObservability(...)`
- registration of logging, tracing, and metrics services
- `IOperationContext` registration

## What it does not contain

- ASP.NET Core middleware
- endpoint mapping
- `HttpClient` correlation propagation

## Usage

```csharp
builder.Services.AddRaycynixObservability();
```

For ASP.NET Core integration, use `Raycynix.Extensions.Observability.AspNetCore`.
`AddRaycynixAspNetCoreObservability(...)` already calls `AddRaycynixObservability()` for you.
