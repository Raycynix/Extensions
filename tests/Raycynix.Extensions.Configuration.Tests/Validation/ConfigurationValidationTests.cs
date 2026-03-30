using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Raycynix.Extensions.Configuration.Tests.Validation;

/// <summary>
/// Covers typed configuration validation behavior.
/// </summary>
public class ConfigurationValidationTests
{
    /// <summary>
    /// Verifies that registered validators fail options resolution when the configuration is invalid.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationValidator_ShouldRejectInvalidOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ValidatedOptions:Endpoint"] = string.Empty,
                ["ValidatedOptions:TimeoutSeconds"] = "0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<ValidatedOptions>(configuration);
        services.AddRaycynixConfigurationValidator<ValidatedOptions>(
            options => !string.IsNullOrWhiteSpace(options.Endpoint) && options.TimeoutSeconds > 0,
            "ValidatedOptions must contain a non-empty endpoint and a positive timeout.");

        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*ValidatedOptions must contain a non-empty endpoint and a positive timeout.*");
    }

    private class ValidatedOptions
    {
        public string Endpoint { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; }
    }
}
