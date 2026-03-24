using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Implementations;

internal sealed class ConfigurationAccessor<TOptions>(IOptionsMonitor<TOptions> optionsMonitor)
    : IConfigurationAccessor<TOptions>
    where TOptions : class
{
    public TOptions Current => optionsMonitor.CurrentValue;

    public TOptions Get(string? name)
    {
        return optionsMonitor.Get(name ?? Options.DefaultName);
    }
}
