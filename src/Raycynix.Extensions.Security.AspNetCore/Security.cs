using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Security.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Models;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Conventions;
using Raycynix.Extensions.Security.AspNetCore.Authorization.PolicyProvider;
using Raycynix.Extensions.Security.AspNetCore.Implementations;
using Raycynix.Extensions.Security.AspNetCore.Internal;
using Raycynix.Extensions.Security.Options;

namespace Raycynix.Extensions.Security.AspNetCore;

/// <summary>
/// Provides ASP.NET Core service registration and middleware extensions for the Raycynix security package.
/// </summary>
public static class Security
{
    /// <summary>
    /// Registers the core security services and ASP.NET Core JWT authentication integration.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="SecurityOptions"/>.</param>
    /// <param name="setup">An optional callback for adjusting the bound security configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecurityOptions>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddRaycynixSecurity(configuration, setup);
        services.AddRaycynixConfigurationValidator<SecurityOptions, AspNetCoreSecurityOptionsValidator>();
        services.AddHttpContextAccessor();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<SecurityOptions>(ConfigureJwtBearer);

        services.AddAuthorization();
        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new RaycynixAuthorizationApplicationModelConvention());
        });
        services.Replace(
            ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, RaycynixAuthorizationPolicyProvider>());
        services.Replace(ServiceDescriptor
            .Singleton<IAuthorizationMiddlewareResultHandler, RaycynixAuthorizationMiddlewareResultHandler>());
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyRoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AllRolesAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SubjectTypeAuthorizationHandler>();

        services.Replace(ServiceDescriptor.Scoped<ISecurityContext>(serviceProvider =>
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var securityContext = HttpSecurityContextFactory.Create(httpContextAccessor.HttpContext?.User);
            var logger = serviceProvider.GetService<ILogger<ISecurityContext>>();
            logger?.LogDebug(
                "Resolved HTTP security context. IsAuthenticated={IsAuthenticated}, SubjectType={SubjectType}, RoleCount={RoleCount}, PermissionCount={PermissionCount}.",
                securityContext.IsAuthenticated,
                securityContext.SubjectType,
                securityContext.Roles.Count,
                securityContext.Permissions.Count);

            return securityContext;
        }));

        return services;
    }

    /// <summary>
    /// Adds the Raycynix authentication middleware to the ASP.NET Core request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixSecurity(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, SecurityOptions config)
    {
        options.Authority = config.JwtOptions.Authority;
        options.RequireHttpsMetadata = config.JwtOptions.RequireHttpsMetadata;
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()
                    ?.CreateLogger("Raycynix.Extensions.Security.AspNetCore.JwtBearer");

                logger?.LogWarning(
                    "JWT authentication challenge handled. Path={Path}, TraceId={TraceId}.",
                    context.HttpContext.Request.Path.Value,
                    context.HttpContext.TraceIdentifier);

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    var response = new AuthorizationErrorResponse(
                        Status: StatusCodes.Status401Unauthorized,
                        Code: "unauthorized",
                        Message: "Authentication is required to access this resource.",
                        TraceId: context.HttpContext.TraceIdentifier);

                    await context.Response.WriteAsJsonAsync(response);
                }
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config.JwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = config.JwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = config.JwtOptions.ClockSkew,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = SecurityClaimTypes.Roles
        };
    }

}
