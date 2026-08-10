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

    /// <summary>
    /// Verifies that rapid configuration changes cannot overtake an earlier handler invocation.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfigurationChangeHandler_ShouldProcessChangesSequentially()
    {
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var observedValues = new ConcurrentQueue<int>();
        var configuration = CreateConfiguration();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<ReloadableOptions>(configuration);
        services.AddRaycynixConfigurationChangeHandler<ReloadableOptions>(async (context, cancellationToken) =>
        {
            if (context.Current.Value == 20)
            {
                firstStarted.TrySetResult();
                await releaseFirst.Task.WaitAsync(cancellationToken);
            }
            else if (context.Current.Value == 30)
            {
                secondStarted.TrySetResult();
            }

            observedValues.Enqueue(context.Current.Value);
        });

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            configuration["ReloadableOptions:Value"] = "20";
            configuration.Reload();
            await firstStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

            configuration["ReloadableOptions:Value"] = "30";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);
            secondStarted.Task.IsCompleted.Should().BeFalse();

            releaseFirst.TrySetResult();
            await secondStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
            observedValues.Should().Equal(20, 30);
        }
        finally
        {
            releaseFirst.TrySetResult();
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Verifies that stopping the host cancels an active configuration change handler.
    /// </summary>
    [Fact]
    public async Task StopAsync_ShouldCancelAndAwaitActiveHandler()
    {
        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerCancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var configuration = CreateConfiguration();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<ReloadableOptions>(configuration);
        services.AddRaycynixConfigurationChangeHandler<ReloadableOptions>(async (_, cancellationToken) =>
        {
            handlerStarted.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                handlerCancelled.TrySetResult();
                throw;
            }
        });

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        configuration["ReloadableOptions:Value"] = "20";
        configuration.Reload();
        await handlerStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        await handlerCancelled.Task.WaitAsync(TestContext.Current.CancellationToken);
    }

    private static IConfigurationRoot CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReloadableOptions:Value"] = "10"
            })
            .Build();
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
