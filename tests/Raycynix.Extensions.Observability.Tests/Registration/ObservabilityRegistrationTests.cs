using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Observability.Tests.Registration;

/// <summary>
/// Covers service registration for the core observability package.
/// </summary>
public class ObservabilityRegistrationTests
{
    /// <summary>
    /// Verifies that the core observability extension registers metrics, tracing,
    /// the logger abstraction descriptor, and the ambient operation context.
    /// </summary>
    [Fact]
    public void AddRaycynixObservability_ShouldRegisterCoreObservabilityServices()
    {
        var services = new ServiceCollection();

        services.AddRaycynixObservability();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var metrics = scope.ServiceProvider.GetRequiredService<IMetricsService>();
        var tracer = scope.ServiceProvider.GetRequiredService<ITracer>();
        var operationContext = scope.ServiceProvider.GetRequiredService<IOperationContext>();
        var loggerDescriptor = services.SingleOrDefault(x => x.ServiceType == typeof(ILogger<>));

        metrics.Should().NotBeNull();
        tracer.Should().NotBeNull();
        operationContext.Should().BeOfType<OperationContext>();
        loggerDescriptor.Should().NotBeNull();
        loggerDescriptor.ImplementationType.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the operation context is scoped and reused within a single scope
    /// while remaining isolated across different scopes.
    /// </summary>
    [Fact]
    public void AddRaycynixObservability_ShouldRegisterOperationContextAsScoped()
    {
        var services = new ServiceCollection();
        services.AddRaycynixObservability();

        var provider = services.BuildServiceProvider();

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var firstInstanceA = firstScope.ServiceProvider.GetRequiredService<IOperationContext>();
        var firstInstanceB = firstScope.ServiceProvider.GetRequiredService<IOperationContext>();
        var secondInstance = secondScope.ServiceProvider.GetRequiredService<IOperationContext>();

        firstInstanceA.Should().BeSameAs(firstInstanceB);
        firstInstanceA.Should().NotBeSameAs(secondInstance);
    }
}
