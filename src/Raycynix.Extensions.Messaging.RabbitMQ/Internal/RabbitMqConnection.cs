using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;


/// <summary>
/// RabbitMQ connection adapter over the client library.
/// </summary>
internal sealed class RabbitMqConnection(IConnection connection) : IRabbitMqConnection
{
    /// <inheritdoc />
    public async Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return new RabbitMqChannel(channel);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync().ConfigureAwait(false);
    }
}
