using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Raycynix.Extensions.Security.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Models;
using Raycynix.Extensions.Security.AspNetCore.Authorization.PolicyProvider;
using Raycynix.Extensions.Security.AspNetCore.Implementations;
using Raycynix.Extensions.Security.Configurations;

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
    /// <param name="configuration">The application configuration used to bind <see cref="SecurityConfiguration"/>.</param>
    /// <param name="setup">An optional callback for adjusting the bound security configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecurityConfiguration>? setup = null)
    {
        var config = new SecurityConfiguration();
        configuration.GetSection(nameof(SecurityConfiguration)).Bind(config);

        setup?.Invoke(config);
        config.Validate();
        ValidateAspNetCoreConfiguration(config);

        services.Replace(ServiceDescriptor.Singleton(config));
        services.AddRaycynixSecurity();
        services.AddHttpContextAccessor();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => ConfigureJwtBearer(options, config));

        services.AddAuthorization();
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, RaycynixAuthorizationPolicyProvider>());
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationMiddlewareResultHandler, RaycynixAuthorizationMiddlewareResultHandler>());
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
            return HttpSecurityContextFactory.Create(httpContextAccessor.HttpContext?.User);
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
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, SecurityConfiguration config)
    {
        options.Authority = config.Jwt.Authority;
        options.RequireHttpsMetadata = config.Jwt.RequireHttpsMetadata;
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

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
            ValidIssuer = config.Jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = config.Jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = config.Jwt.ClockSkew,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = SecurityClaimTypes.Roles
        };
    }

    private static void ValidateAspNetCoreConfiguration(SecurityConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.Jwt.Authority))
        {
            throw new InvalidOperationException(
                "SecurityConfiguration.Jwt.Authority must be provided for ASP.NET Core JWT validation.");
        }
    }
}
