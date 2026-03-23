# Raycynix.Extensions.Security

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Security` contains the core, host-agnostic security implementation for Raycynix applications.

## What it contains

- `SecurityContext`
- `AddRaycynixSecurity(...)`
- DI registration for `ISecurityContext`

## What it does not contain

- ASP.NET Core authentication handlers
- HTTP request mapping
- JWT validation middleware
- authorization policies

## Usage

```csharp
builder.Services.AddRaycynixSecurity();
```

For ASP.NET Core request binding and web-specific integration, use `Raycynix.Extensions.Security.AspNetCore`.
