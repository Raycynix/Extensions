using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Default RabbitMQ connection factory backed by RabbitMQ.Client.
/// </summary>
internal sealed class RabbitMqClientConnectionFactory : IRabbitMqConnectionFactory
{
    /// <inheritdoc />
    public async Task<IRabbitMqConnection> CreateAsync(
        RabbitMqMessagingConfiguration configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var factory = new ConnectionFactory
        {
            HostName = configuration.Host,
            Port = configuration.Port,
            VirtualHost = configuration.VirtualHost,
            UserName = configuration.Username,
            Password = configuration.Password,
            ConsumerDispatchConcurrency = 1
        };

        if (configuration.UseSsl)
        {
            factory.Ssl = new SslOption { Enabled = true, ServerName = configuration.Host };
        }

        var connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        return new RabbitMqConnection(connection);
    }
}
