using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Elastic;
using Raycynix.Extensions.Logging.Elastic.Options;

namespace Raycynix.Extensions.Logging.Tests.Registration;

/// <summary>
/// Covers optional logging integration registration.
/// </summary>
public sealed class LoggingIntegrationRegistrationTests
{
    [Fact]
    public void AddElastic_ShouldBindNestedOptions()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["LoggingOptions:ElasticOptions:Enabled"] = "true",
            ["LoggingOptions:ElasticOptions:Url"] = "https://elastic.example:9200"
        });

        services.AddRaycynixLogging(configuration).AddElastic();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ElasticOptions>();

        options.Enabled.Should().BeTrue();
        options.Url.Should().Be("https://elastic.example:9200");
        provider.GetServices<IRaycynixLoggingConfigurator>().Should().ContainSingle();
    }

    [Fact]
    public void AddElastic_WithNonHttpEndpoint_ShouldFailValidation()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["LoggingOptions:ElasticOptions:Enabled"] = "true",
            ["LoggingOptions:ElasticOptions:Url"] = "ftp://elastic.example"
        });

        services.AddRaycynixLogging(configuration).AddElastic();

        using var provider = services.BuildServiceProvider();
        var access = () => provider.GetRequiredService<IOptions<ElasticOptions>>().Value;

        access.Should().Throw<OptionsValidationException>()
            .WithMessage("*HTTP or HTTPS*");
    }

    [Fact]
    public async Task AddElastic_ShouldNotCreateLoggingProviderDependencyCycle()
    {
        using var host = Host.CreateDefaultBuilder()
            .UseRaycynixLogging()
            .ConfigureServices((context, services) =>
                services.AddRaycynixLogging(context.Configuration).AddElastic())
            .Build();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await host.StartAsync(timeout.Token);
        await host.StopAsync(timeout.Token);
    }

    [Fact]
    public void AddElastic_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration([]);
        var builder = services.AddRaycynixLogging(configuration);

        builder.AddElastic();
        builder.AddElastic();

        services.Count(descriptor =>
                descriptor.ServiceType == typeof(IRaycynixLoggingConfigurator))
            .Should().Be(1);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
