using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class DelegateConfigurationChangeHandler<TOptions>(
    Func<ConfigurationChangeContext<TOptions>, CancellationToken, ValueTask> handleAsync)
    : IConfigurationChangeHandler<TOptions>
    where TOptions : class
{
    public ValueTask HandleAsync(
        ConfigurationChangeContext<TOptions> context,
        CancellationToken cancellationToken = default)
    {
        return handleAsync(context, cancellationToken);
    }
}
