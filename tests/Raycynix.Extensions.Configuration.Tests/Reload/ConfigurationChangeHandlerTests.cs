using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Tests.Reload;

/// <summary>
/// Covers runtime configuration change handler execution.
/// </summary>
public class ConfigurationChangeHandlerTests
{
    /// <summary>
    /// Verifies that change handlers are invoked for runtime updates that are allowed to apply.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfigurationChangeHandler_ShouldInvokeHandlersWhenReloadIsApplied()
    {
        var observedValues = new ConcurrentQueue<int>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReloadableOptions:Value"] = "10"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<ReloadableOptions>(configuration);
        services.AddRaycynixConfigurationChangeHandler<ReloadableOptions>(
            static (context, cancellationToken) =>
            {
                HandlerState.Values.Enqueue(context.Current.Value);
                return ValueTask.CompletedTask;
            });

        HandlerState.Values = observedValues;

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            configuration["ReloadableOptions:Value"] = "25";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);

            observedValues.Should().ContainSingle().Which.Should().Be(25);
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    private static async Task StartHostedServicesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(cancellationToken);
        }
    }

    private static async Task StopHostedServicesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StopAsync(cancellationToken);
        }
    }

    private static class HandlerState
    {
        public static ConcurrentQueue<int> Values { get; set; } = new();
    }

    private class ReloadableOptions
    {
        public int Value { get; set; }
    }
}
