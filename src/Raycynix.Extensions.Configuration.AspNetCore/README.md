# Raycynix.Extensions.Configuration.AspNetCore

`Raycynix.Extensions.Configuration.AspNetCore` adds ASP.NET Core-specific integrations for Raycynix configuration and feature flags.

## What it contains

- `AddRaycynixAspNetCoreConfiguration(...)`
- `AddRaycynixFeatureGateOptions(...)`
- `UseRaycynixAspNetCoreConfiguration(...)`
- `[FeatureGate(...)]`
- `RequireFeature(...)`
- `RequireAnyFeature(...)`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixAspNetCoreConfiguration();
builder.Services.AddRaycynixFeatureGateOptions(options =>
{
    options.DisabledStatusCode = StatusCodes.Status404NotFound;
    options.MissingFeatureFlagAccessorStatusCode = StatusCodes.Status503ServiceUnavailable;
});

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

When a required feature flag is disabled, the request returns `404 Not Found` by default. When a gated endpoint is reached without a registered `IFeatureFlagAccessor`, the middleware also returns `404 Not Found` by default. Both status codes can be changed through `AddRaycynixFeatureGateOptions(...)`.
