using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Adapts an inline delegate to <see cref="IConfigurationChangeHandler{TOptions}"/>.
/// </summary>
internal sealed class DelegateConfigurationChangeHandler<TOptions>(
    Func<ConfigurationChangeContext<TOptions>, CancellationToken, ValueTask> handleAsync)
    : IConfigurationChangeHandler<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ValueTask HandleAsync(
        ConfigurationChangeContext<TOptions> context,
        CancellationToken cancellationToken = default)
    {
        return handleAsync(context, cancellationToken);
    }
}
