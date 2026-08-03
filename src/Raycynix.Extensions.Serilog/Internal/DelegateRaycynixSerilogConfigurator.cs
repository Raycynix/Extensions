using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Contexts;
using Serilog;

namespace Raycynix.Extensions.Serilog.Internal;

internal sealed class DelegateRaycynixSerilogConfigurator(
    Action<RaycynixSerilogContext, LoggerConfiguration> configure,
    int order)
    : IRaycynixSerilogConfigurator
{
    private readonly Action<
        RaycynixSerilogContext,
        LoggerConfiguration> _configure =
        configure ?? throw new ArgumentNullException(nameof(configure));

    public int Order { get; } = order;

    public void Configure(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogContext context)
    {
        _configure(context, loggerConfiguration);
    }
}