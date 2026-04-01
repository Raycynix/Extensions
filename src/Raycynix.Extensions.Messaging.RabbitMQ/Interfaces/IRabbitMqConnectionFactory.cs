using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

/// <summary>
/// Creates RabbitMQ connections for the transport.
/// </summary>
public interface IRabbitMqConnectionFactory
{
    /// <summary>
    /// Creates a new RabbitMQ connection.
    /// </summary>
    /// <param name="configuration">The RabbitMQ transport configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A connected RabbitMQ connection abstraction.</returns>
    Task<IRabbitMqConnection> CreateAsync(RabbitMqMessagingConfiguration configuration, CancellationToken cancellationToken);
}
