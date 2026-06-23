using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Implementations;

namespace Raycynix.Extensions.Tracing.Tests.Registration;

/// <summary>
/// Covers dependency registration for the tracing package.
/// </summary>
public sealed class TracingRegistrationTests
{
    /// <summary>
    /// Verifies that the shared tracer abstraction is registered as a singleton.
    /// </summary>
    [Fact]
    public void AddRaycynixTracing_ShouldRegisterTracer()
    {
        var services = new ServiceCollection();

        services.AddRaycynixTracing();

        var descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(ITracer))
            .Subject;

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationFactory.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that repeated registration does not duplicate the shared tracer abstraction.
    /// </summary>
    [Fact]
    public void AddRaycynixTracing_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddRaycynixTracing();
        services.AddRaycynixTracing();

        services.Count(service => service.ServiceType == typeof(ITracer)).Should().Be(1);
    }

    /// <summary>
    /// Verifies that the shared tracer abstraction can be resolved from DI.
    /// </summary>
    [Fact]
    public void AddRaycynixTracing_ShouldResolveTracer()
    {
        var services = new ServiceCollection();
        services.AddRaycynixTracing();

        using var provider = services.BuildServiceProvider();
        var tracer = provider.GetRequiredService<ITracer>();

        tracer.Should().BeOfType<Tracer>();
    }
}