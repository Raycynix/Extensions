using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Configuration.Configurations;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class ConfigurationDiagnosticsStore(ConfigurationDiagnosticsOptions options)
{
    private readonly Lock _sync = new();
    private readonly List<ConfigurationRegistrationInfo> _registrations = [];
    private readonly Dictionary<(Type OptionsType, string OptionsName), Queue<ConfigurationReloadInfo>> _reloads = [];

    public void AddRegistration(ConfigurationRegistrationInfo registration)
    {
        lock (_sync)
        {
            if (_registrations.Any(current =>
                    current.OptionsType == registration.OptionsType &&
                    string.Equals(current.OptionsName, registration.OptionsName, StringComparison.Ordinal)))
            {
                return;
            }

            _registrations.Add(registration);
        }
    }

    public void SetReload(
        Type optionsType,
        string optionsName,
        ConfigurationReloadBehavior behavior,
        string? reason)
    {
        lock (_sync)
        {
            var key = (optionsType, optionsName);
            if (!_reloads.TryGetValue(key, out var history))
            {
                history = new Queue<ConfigurationReloadInfo>();
                _reloads[key] = history;
            }

            history.Enqueue(new ConfigurationReloadInfo(
                optionsType,
                optionsName,
                behavior,
                reason,
                DateTimeOffset.UtcNow));

            var maxHistory = Math.Max(1, options.MaxReloadHistoryPerOptions);
            while (history.Count > maxHistory)
            {
                history.Dequeue();
            }
        }
    }

    public IReadOnlyCollection<ConfigurationRegistrationInfo> GetRegistrations()
    {
        lock (_sync)
            return _registrations.ToArray();
    }

    public IReadOnlyCollection<ConfigurationReloadInfo> GetReloads()
    {
        lock (_sync)
            return _reloads.Values.SelectMany(static history => history).ToArray();
    }
}
