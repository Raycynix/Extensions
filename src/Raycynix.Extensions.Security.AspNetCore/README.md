# Raycynix.Extensions.Security.AspNetCore

`Raycynix.Extensions.Security.AspNetCore` adds ASP.NET Core JWT authentication, dynamic authorization policies, and shared authorization-attribute integration for Raycynix security.

## What it contains

- `AddRaycynixAspNetCoreSecurity(...)`
- `UseRaycynixSecurity(this IApplicationBuilder app)`
- per-request `ClaimsPrincipal` to `ISecurityContext` mapping
- dynamic API policies for `authenticated`, `permission:*`, `role:*`, and `subject:*`
- MVC convention support for shared security attributes from `Raycynix.Extensions.Security.Abstractions`
- endpoint-builder helpers through `RequireRaycynixAuthorization(...)`
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

```json
{
  "SecurityConfiguration": {
    "Jwt": {
      "Authority": "https://auth.raycynix.com",
      "Issuer": "raycynix-auth",
      "Audience": "raycynix-services",
      "RequireHttpsMetadata": true
    }
  }
}
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
[Authorize(Policy = SecurityPolicies.Authenticated)]
[Authorize(Policy = SecurityPolicies.ServiceOnly)]
```

Or use the shared security attributes and let the package translate them into standard ASP.NET Core authorization policies:

```csharp
using Raycynix.Extensions.Security.Abstractions.Attributes;

[RequireAuthenticatedSubject]
[RequireSubjectType(SecuritySubjectType.Service)]
[RequirePermission("users.read")]
public sealed class UsersController : ControllerBase
{
}
```

For minimal APIs or endpoint builders, use the helper extension:

```csharp
app.MapGet("/users/{id}", HandleUserAsync)
    .RequireRaycynixAuthorization(
        new RequireAuthenticatedSubjectAttribute(),
        new RequirePermissionAttribute("users.read"));
```

The package expects JWT access tokens with:

- `sub`
- `subject_type`
- `roles`
- `permissions`

`subject_type` is mapped to `SecuritySubjectType`, allowing both `User` and `Service` request subjects to use the same `ISecurityContext`.

Authentication and authorization failures return safe JSON responses without exposing internal policy details.
