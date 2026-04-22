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
        services.Should().ContainSingle(service => service.ServiceType == typeof(ISecretDiagnosticsResolver));
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
        services.Count(service => service.ServiceType == typeof(ISecretDiagnosticsResolver)).Should().Be(1);
    }

    /// <summary>
    /// Verifies that the diagnostics and standard resolver interfaces point to the same singleton instance.
    /// </summary>
    [Fact]
    public void AddRaycynixSecrets_ShouldResolveSameInstance_ForResolverAndDiagnosticsResolver()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddRaycynixSecrets();

        using var serviceProvider = services.BuildServiceProvider();

        var resolver = serviceProvider.GetRequiredService<ISecretResolver>();
        var diagnosticsResolver = serviceProvider.GetRequiredService<ISecretDiagnosticsResolver>();

        resolver.Should().BeSameAs(diagnosticsResolver);
    }
}
