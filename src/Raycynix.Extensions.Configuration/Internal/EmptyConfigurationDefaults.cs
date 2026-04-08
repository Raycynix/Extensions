using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Placeholder defaults provider used when no custom defaults are registered.
/// </summary>
internal sealed class EmptyConfigurationDefaults<TOptions> : IConfigurationDefaults<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public void Apply(TOptions options)
    {
    }
}
