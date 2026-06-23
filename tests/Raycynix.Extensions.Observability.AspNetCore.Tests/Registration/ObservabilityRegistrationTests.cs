using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Configurations;
using Raycynix.Extensions.Observability.AspNetCore.Http;

namespace Raycynix.Extensions.Observability.AspNetCore.Tests.Registration;

/// <summary>
/// Covers ASP.NET Core service registration for observability integrations.
/// </summary>
public class ObservabilityRegistrationTests
{
    /// <summary>
    /// Verifies that the ASP.NET Core observability extension registers
    /// the HTTP context accessor, correlation handler, and HTTP client builder filter.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreObservability_ShouldRegisterAspNetCoreIntegrations()
    {
        var services = new ServiceCollection();

        services.AddRaycynixAspNetCoreObservability();

        var provider = services.BuildServiceProvider();

        var operationContext = provider.GetRequiredService<IOperationContext>();
        var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        var handler = provider.GetRequiredService<CorrelationHeaderHandler>();
        var filters = provider.GetServices<IHttpMessageHandlerBuilderFilter>();

        operationContext.Should().NotBeNull();
        httpContextAccessor.Should().NotBeNull();
        handler.Should().NotBeNull();
        filters.Should().ContainSingle(x => x.GetType().Name == "CorrelationHttpMessageHandlerBuilderFilter");
    }

    /// <summary>
    /// Verifies that the ASP.NET Core observability registration can be applied repeatedly without duplicating the HTTP builder filter.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreObservability_ShouldBeIdempotent_ForHttpBuilderFilter()
    {
        var services = new ServiceCollection();

        services.AddRaycynixAspNetCoreObservability();
        services.AddRaycynixAspNetCoreObservability();

        services.Count(service => service.ServiceType == typeof(IHttpMessageHandlerBuilderFilter)).Should().Be(1);
    }

    /// <summary>
    /// Verifies that ASP.NET Core observability options can be configured during registration.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreObservability_ShouldConfigureOptions()
    {
        var services = new ServiceCollection();

        services.AddRaycynixAspNetCoreObservability(options =>
        {
            options.IncludeIdentityInLoggingScope = false;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ObservabilityAspNetCoreConfiguration>>().Value;

        options.IncludeIdentityInLoggingScope.Should().BeFalse();
    }
}
