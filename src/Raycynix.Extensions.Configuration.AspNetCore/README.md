# Raycynix.Extensions.Configuration.AspNetCore

`Raycynix.Extensions.Configuration.AspNetCore` adds ASP.NET Core-specific integrations for Raycynix configuration and feature flags.

## What it contains

- `AddRaycynixAspNetCoreConfiguration(...)`
- `UseRaycynixAspNetCoreConfiguration(...)`
- `[FeatureGate(...)]`
- `RequireFeature(...)`
- `RequireAnyFeature(...)`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixAspNetCoreConfiguration();

builder.Services.AddRaycynixFeatureFlags(builder.Configuration);

var app = builder.Build();

app.UseRaycynixAspNetCoreConfiguration();

app.MapGet("/dashboard", () => Results.Ok("enabled"))
    .RequireFeature("NewDashboard");

app.Run();
```

## appsettings.json

```json
{
  "FeatureFlags": {
    "Flags": {
      "NewDashboard": true,
      "BetaApi": false
    }
  }
}
```

Use the attribute for MVC or endpoint metadata scenarios:

```csharp
[FeatureGate("NewDashboard")]
public sealed class DashboardController : ControllerBase
{
}
```

When a required feature flag is disabled, the request returns `404 Not Found`.
