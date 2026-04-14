using Raycynix.Extensions.Configuration.Abstractions.Attributes;
using Raycynix.Extensions.Configuration.Abstractions.Enums;

namespace Raycynix.Extensions.Configuration.Example;

internal sealed class MessagingOptions
{
    [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
    public string ConnectionString { get; set; } = string.Empty;

    public string ConsumerName { get; set; } = string.Empty;

    public int BatchSize { get; set; }

    public int PrefetchCount { get; set; }

    public bool UseInboxProcessing { get; set; }
}