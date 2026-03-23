# Raycynix.Extensions.Security

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Security` contains the core, host-agnostic security implementation for Raycynix applications.

## What it contains

- `SecurityContext`
- `AddRaycynixSecurity(...)`
- `SecurityConfiguration`
- `JwtConfiguration`
- DI registration for `ISecurityContext`

## What it does not contain

- ASP.NET Core authentication handlers
- HTTP request mapping
- JWT validation middleware
- authorization policies
- secret storage and resolution

## Usage

```csharp
builder.Services.AddRaycynixSecurity(builder.Configuration, options =>
{
    options.Jwt.Authority = "https://auth.raycynix.com";
    options.Jwt.Issuer = "raycynix-auth";
    options.Jwt.Audience = "raycynix-services";
});
```

The package binds settings from the `SecurityConfiguration` section and allows optional overrides in code.

For ASP.NET Core request binding and web-specific integration, use `Raycynix.Extensions.Security.AspNetCore`.

For secret resolution, use `Raycynix.Extensions.Secrets`.
