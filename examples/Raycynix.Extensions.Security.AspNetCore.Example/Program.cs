using System.IdentityModel.Tokens.Jwt;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore;
using Raycynix.Extensions.Security.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Options;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreSecurity(builder.Configuration, options =>
{
    options.JwtOptions.RequireHttpsMetadata = false;
});
builder.Services.AddRaycynixRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseRaycynixRateLimiting();
app.UseAuthorization();

app.MapGet("/", (SecurityOptions options) => Results.Ok(new
{
    message = "Raycynix Security ASP.NET Core example",
    issuer = options.JwtOptions.Issuer,
    audience = options.JwtOptions.Audience,
    protectedEndpoints = new[]
    {
        "/profile",
        "/permissions/export",
        "/services/dispatch"
    }
}));

app.MapGet("/profile", (ISecurityContext securityContext) => Results.Ok(new
{
    securityContext.IsAuthenticated,
    securityContext.SubjectId,
    SubjectType = securityContext.SubjectType.ToString(),
    securityContext.Roles,
    securityContext.Permissions
}))
    .RequireRaycynixAuthorization(new RequireAuthenticatedSubjectAttribute());

app.MapGet("/permissions/export", (ISecurityContext securityContext) => Results.Ok(new
{
    securityContext.SubjectId,
    requiredPermission = "reports.export"
}))
    .RequireAuthorization(SecurityPolicies.Permission("reports.export"));

app.MapGet("/services/dispatch", (ISecurityContext securityContext) => Results.Ok(new
{
    securityContext.SubjectId,
    requiredSubjectType = SecuritySubjectType.Service.ToString()
}))
    .RequireRaycynixAuthorization(
        new RequireAuthenticatedSubjectAttribute(),
        new RequireSubjectTypeAttribute(SecuritySubjectType.Service));

app.MapGet("/token-shape", () => Results.Ok(new
{
    requiredClaims = new[]
    {
        JwtRegisteredClaimNames.Sub,
        SecurityClaimTypes.SubjectType,
        SecurityClaimTypes.Roles,
        SecurityClaimTypes.Permissions
    },
    sample = new
    {
        sub = "user-123",
        subject_type = "User",
        roles = new[] { "admin", "report-viewer" },
        permissions = new[] { "reports.read", "reports.export" }
    }
}))
    .RequireRateLimiting("sensitive");

app.Run();
