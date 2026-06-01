using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Bridges <see cref="IOptionsMonitor{TOptions}"/> changes into Raycynix reload policies and change handlers.
/// </summary>
internal sealed class ConfigurationChangeHostedService<TOptions>(
    IOptionsMonitor<TOptions> optionsMonitor,
    ConfigurationRuntimeState<TOptions> runtimeState,
    ConfigurationDiagnosticsStore diagnostics,
    IEnumerable<ConfigurationOptionsRegistration<TOptions>> registrations,
    IEnumerable<IConfigurationReloadPolicy<TOptions>> reloadPolicies,
    IEnumerable<IConfigurationChangeHandler<TOptions>> handlers,
    ILogger<ConfigurationChangeHostedService<TOptions>> logger)
    : IHostedService, IDisposable
    where TOptions : class
{
    private IDisposable? _registration;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        runtimeState.SetCurrent(Options.DefaultName, optionsMonitor.CurrentValue);
        _registration = optionsMonitor.OnChange(OnChanged);

        foreach (var registration in registrations)
        {
            diagnostics.AddRegistration(new ConfigurationRegistrationInfo(
                typeof(TOptions),
                registration.SectionName,
                registration.Name,
                registration.RequiredSection));
            runtimeState.SetCurrent(registration.Name, optionsMonitor.Get(registration.Name));
        }

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
        var optionsName = string.IsNullOrWhiteSpace(name) ? Options.DefaultName : name;
        var previousOptions = runtimeState.GetCurrent(optionsName) ?? optionsMonitor.Get(optionsName);
        var context = new ConfigurationChangeContext<TOptions>(previousOptions, updatedOptions, optionsName);
        var reloadResult = EvaluateReload(context);
        diagnostics.SetReload(
            typeof(TOptions),
            optionsName,
            reloadResult.Behavior,
            reloadResult.Reason);

        switch (reloadResult.Behavior)
        {
            case ConfigurationReloadBehavior.Apply:
                runtimeState.SetCurrent(optionsName, updatedOptions);
                logger.LogInformation(
                    "A runtime configuration change for options type {OptionsType} was applied.",
                    typeof(TOptions).Name);
                _ = NotifyHandlersAsync(context);
                return;

            case ConfigurationReloadBehavior.Reject:
                logger.LogWarning(
                    "A runtime configuration change for options type {OptionsType} was rejected. {Reason}",
                    typeof(TOptions).Name,
                    reloadResult.Reason ?? "No reason was provided.");
                return;
            case ConfigurationReloadBehavior.Ignore:
                logger.LogInformation("A runtime configuration for options type {OptionsType} was ignored. {Reason}",
                    typeof(TOptions).Name,
                    reloadResult.Reason ?? "No reason was provided.");
                return;

            case ConfigurationReloadBehavior.RestartRequired:
                logger.LogWarning(
                    "A runtime configuration change for options type {OptionsType} requires an application restart. {Reason}",
                    typeof(TOptions).Name,
                    reloadResult.Reason ?? "No reason was provided.");
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private ConfigurationReloadResult EvaluateReload(ConfigurationChangeContext<TOptions> context)
    {
        foreach (var policy in reloadPolicies)
        {
            var result = policy.Evaluate(context);
            if (result.Behavior != ConfigurationReloadBehavior.Apply)
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