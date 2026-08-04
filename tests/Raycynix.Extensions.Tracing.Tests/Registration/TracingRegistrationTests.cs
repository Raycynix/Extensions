using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.Tests.Registration;

/// <summary>
/// Covers dependency registration for the tracing package.
/// </summary>
public sealed class TracingRegistrationTests
{
    [Fact]
    public void AddRaycynixTracing_ShouldRegisterSharedActivitySource()
    {
        var services = new ServiceCollection();

        services.AddRaycynixTracing();

        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(ActivitySource) &&
            ReferenceEquals(descriptor.ImplementationInstance, RaycynixTracing.ActivitySource));
    }

    [Fact]
    public void AddRaycynixTracing_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddRaycynixTracing();
        services.AddRaycynixTracing();

        services.Count(descriptor =>
                descriptor.ServiceType == typeof(ActivitySource) &&
                ReferenceEquals(descriptor.ImplementationInstance, RaycynixTracing.ActivitySource))
            .Should().Be(1);
    }

    [Fact]
    public void AddRaycynixTracing_ShouldResolveSharedActivitySource()
    {
        var services = new ServiceCollection();
        services.AddRaycynixTracing();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ActivitySource>().Should().BeSameAs(RaycynixTracing.ActivitySource);
    }
}
