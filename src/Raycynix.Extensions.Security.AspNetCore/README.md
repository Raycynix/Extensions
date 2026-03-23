# Raycynix.Extensions.Security.AspNetCore

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Security.AspNetCore` adds ASP.NET Core JWT authentication integration for Raycynix security.

## What it contains

- `AddRaycynixAspNetCoreSecurity(...)`
- `UseRaycynixSecurity(this IApplicationBuilder app)`
- per-request `ClaimsPrincipal` to `ISecurityContext` mapping
- dynamic API policies for `permission:*`, `role:*`, and `subject:*`
- consistent `401 Unauthorized` and `403 Forbidden` JSON responses

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreSecurity(builder.Configuration, options =>
{
    options.Jwt.Authority = "https://auth.raycynix.com";
    options.Jwt.Issuer = "raycynix-auth";
    options.Jwt.Audience = "raycynix-services";
});

var app = builder.Build();

app.UseRaycynixSecurity();

app.Run();
```

Use authorization policies with standard names:

```csharp
using Raycynix.Extensions.Security.AspNetCore.Authorization;

[Authorize(Policy = SecurityPolicies.Permission("users.read"))]
[Authorize(Policy = SecurityPolicies.AnyPermission("users.read", "users.write"))]
[Authorize(Policy = SecurityPolicies.AllPermissions("users.read", "users.export"))]
[Authorize(Policy = SecurityPolicies.Role("admin"))]
[Authorize(Policy = SecurityPolicies.AnyRole("admin", "support"))]
[Authorize(Policy = SecurityPolicies.AllRoles("manager", "auditor"))]
[Authorize(Policy = SecurityPolicies.ServiceOnly)]
```

The package expects JWT access tokens with:

- `sub`
- `subject_type`
- `roles`
- `permissions`

`subject_type` is mapped to `SecuritySubjectType`, allowing both `User` and `Service` request subjects to use the same `ISecurityContext`.

Authentication and authorization failures return safe JSON responses without exposing internal policy details.
