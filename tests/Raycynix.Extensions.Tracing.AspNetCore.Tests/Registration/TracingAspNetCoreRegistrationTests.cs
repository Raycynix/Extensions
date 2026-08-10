using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Raycynix.Extensions.Tracing.AspNetCore.Tests.Registration;

/// <summary>
/// Covers OpenTelemetry ASP.NET Core tracing registration.
/// </summary>
public sealed class TracingAspNetCoreRegistrationTests
{
    [Fact]
    public void AddRaycynixAspNetCoreTracing_ShouldInvokeProviderConfiguration()
    {
        var services = new ServiceCollection();
        var invoked = false;

        services.AddRaycynixAspNetCoreTracing(_ => invoked = true);

        invoked.Should().BeTrue();
    }

    [Fact]
    public void AddRaycynixAspNetCoreTracing_ShouldEnableStandardActivityLoggingScopes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixAspNetCoreTracing();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<LoggerFactoryOptions>>().Value;

        options.ActivityTrackingOptions.Should().HaveFlag(ActivityTrackingOptions.TraceId);
        options.ActivityTrackingOptions.Should().HaveFlag(ActivityTrackingOptions.SpanId);
    }
}
