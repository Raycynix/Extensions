using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Creates RabbitMQ connections for the transport.
/// </summary>
internal interface IRabbitMqConnectionFactory
{
    /// <summary>
    /// Creates a new RabbitMQ connection.
    /// </summary>
    Task<IRabbitMqConnection> CreateAsync(RabbitMqMessagingConfiguration configuration, CancellationToken cancellationToken);
}
