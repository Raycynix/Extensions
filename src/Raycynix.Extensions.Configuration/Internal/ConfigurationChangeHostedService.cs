using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Bridges <see cref="IOptionsMonitor{TOptions}"/> changes into Raycynix reload policies and change handlers.
/// </summary>
internal sealed class ConfigurationChangeHostedService<TOptions>(
    IOptionsMonitor<TOptions> optionsMonitor,
    ConfigurationRuntimeState<TOptions> runtimeState,
    IEnumerable<IConfigurationReloadPolicy<TOptions>> reloadPolicies,
    IEnumerable<IConfigurationChangeHandler<TOptions>> handlers,
    ILogger<ConfigurationChangeHostedService<TOptions>> logger)
    : IHostedService, IDisposable
    where TOptions : class
{
    private IDisposable? _registration;
    private TOptions? _currentValue;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _currentValue = optionsMonitor.CurrentValue;
        runtimeState.SetCurrent(_currentValue);
        _registration = optionsMonitor.OnChange(OnChanged);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _registration?.Dispose();
        _registration = null;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _registration?.Dispose();
    }

    private void OnChanged(TOptions updatedOptions, string? name)
    {
        var previousOptions = _currentValue ?? updatedOptions;
        var context = new ConfigurationChangeContext<TOptions>(previousOptions, updatedOptions, name);
        var reloadResult = EvaluateReload(context);

        switch (reloadResult.Behavior)
        {
            case Abstractions.Enums.ConfigurationReloadBehavior.Apply:
                _currentValue = updatedOptions;
                runtimeState.SetCurrent(updatedOptions);
                logger.LogInformation(
                    "A runtime configuration change for options type {OptionsType} was applied.",
                    typeof(TOptions).Name);
                _ = NotifyHandlersAsync(context);
                return;

            case Abstractions.Enums.ConfigurationReloadBehavior.Reject:
                logger.LogWarning(
                    "A runtime configuration change for options type {OptionsType} was rejected. {Reason}",
                    typeof(TOptions).Name,
                    reloadResult.Reason ?? "No reason was provided.");
                return;
        }
    }

    private ConfigurationReloadResult EvaluateReload(ConfigurationChangeContext<TOptions> context)
    {
        foreach (var policy in reloadPolicies)
        {
            var result = policy.Evaluate(context);
            if (result.Behavior != Abstractions.Enums.ConfigurationReloadBehavior.Apply)
            {
                return result;
            }
        }

        return ConfigurationReloadResult.Apply();
    }

    private async Task NotifyHandlersAsync(ConfigurationChangeContext<TOptions> context)
    {
        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An error occurred while handling a configuration change for options type {OptionsType}.",
                    typeof(TOptions).Name);
            }
        }
    }
}
