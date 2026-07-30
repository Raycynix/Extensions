using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Secrets.Options;
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
        var configuration = new ConfigurationBuilder().Build();

        services.AddRaycynixSecrets(configuration);

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
        var configuration = new ConfigurationBuilder().Build();

        services.AddRaycynixSecrets(configuration);
        services.AddRaycynixSecrets(configuration);

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
        var configuration = new ConfigurationBuilder().Build();
        services.AddRaycynixSecrets(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        var resolver = serviceProvider.GetRequiredService<ISecretResolver>();
        var diagnosticsResolver = serviceProvider.GetRequiredService<ISecretDiagnosticsResolver>();

        resolver.Should().BeSameAs(diagnosticsResolver);
    }

    /// <summary>
    /// Verifies that provider order is bound from the conventional SecretOptions section.
    /// </summary>
    [Fact]
    public void AddRaycynixSecrets_ShouldBindProviderOrderFromConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretOptions:ProviderOrder:0"] = SecretProviderNames.GitHub,
                ["SecretOptions:ProviderOrder:1"] = SecretProviderNames.Configuration
            })
            .Build();

        services.AddRaycynixSecrets(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<SecretOptions>();
        var standardOptions = serviceProvider.GetRequiredService<IOptions<SecretOptions>>().Value;
        var accessor = serviceProvider.GetRequiredService<IConfigurationAccessor<SecretOptions>>();

        options.Should().BeSameAs(accessor.Current);
        options.ProviderOrder.Should().Equal(
            SecretProviderNames.GitHub,
            SecretProviderNames.Configuration);
        standardOptions.ProviderOrder.Should().Equal(options.ProviderOrder);
    }

    /// <summary>
    /// Verifies that duplicate provider names are rejected through options validation.
    /// </summary>
    [Fact]
    public void AddRaycynixSecrets_ShouldRejectDuplicateProviderNames()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretOptions:ProviderOrder:0"] = SecretProviderNames.GitHub,
                ["SecretOptions:ProviderOrder:1"] = "github"
            })
            .Build();

        services.AddRaycynixSecrets(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        var action = () => serviceProvider.GetRequiredService<SecretOptions>();

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*duplicate provider name 'github'*");
    }
}
