using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class EmptyConfigurationDefaults<TOptions> : IConfigurationDefaults<TOptions>
    where TOptions : class
{
    public void Apply(TOptions options)
    {
    }
}
