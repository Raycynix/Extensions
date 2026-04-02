using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Implementations;

namespace Raycynix.Extensions.Logging.Tests.Registration;

/// <summary>
/// Covers dependency registration for the Raycynix logging package.
/// </summary>
public sealed class LoggingRegistrationTests
{
    /// <summary>
    /// Verifies that the typed logger abstraction is registered as a singleton.
    /// </summary>
    [Fact]
    public void AddRaycynixLogging_ShouldRegisterTypedLoggerAbstraction()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Serilog.ILogger>(_ => Serilog.Log.Logger);

        services.AddRaycynixLogging();

        var descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(ILogger<>))
            .Subject;

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(Logger<>));
    }

    private sealed class TestCategory;
}
