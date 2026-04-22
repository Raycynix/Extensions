using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Tests.Registration;

/// <summary>
/// Covers dependency registration for the secrets package.
/// </summary>
public sealed class SecretsRegistrationTests
{
    /// <summary>
    /// Verifies that the default provider chain and composite resolver are registered.
    /// </summary>
    [Fact]
    public void AddRaycynixSecrets_ShouldRegisterProvidersAndResolver()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddRaycynixSecrets();

        services.Count(service => service.ServiceType == typeof(ISecretProvider)).Should().Be(4);
        services.Should().ContainSingle(service => service.ServiceType == typeof(ISecretResolver));
    }

    /// <summary>
    /// Verifies that repeated registration does not duplicate the default providers or resolver.
    /// </summary>
    [Fact]
    public void AddRaycynixSecrets_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddRaycynixSecrets();
        services.AddRaycynixSecrets();

        services.Count(service => service.ServiceType == typeof(ISecretProvider)).Should().Be(4);
        services.Count(service => service.ServiceType == typeof(ISecretResolver)).Should().Be(1);
    }
}
