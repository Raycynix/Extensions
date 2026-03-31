# Raycynix.Extensions.Messaging.RabbitMQ

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Messaging.RabbitMQ` contains the RabbitMQ transport integration for Raycynix messaging.

## What it contains

- `AddRabbitMq(...)`
- `RabbitMqMessagingConfiguration`
- `RabbitMqExchangeConfiguration`
- `RabbitMqQueueConfiguration`
- `RabbitMqRetryConfiguration`
- `RabbitMqDeadLetterConfiguration`
- RabbitMQ-backed `IMessagePublisher`
- automatic exchange, queue, binding, and dead-letter topology bootstrap
- message publishing to RabbitMQ exchanges using `MessageEnvelope<TMessage>.Destination` as the routing key

## What it does not contain

- RabbitMQ consumer hosted services
- retry execution pipeline
- poison-message processing workers
- outbox persistence
- inbox/idempotency storage

## Usage

Register the transport:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddRabbitMq(options =>
    {
        options.Host = "localhost";
        options.Port = 5672;
        options.Exchange.Name = "integration.events";
        options.Exchange.Type = "topic";
        options.Queue.Name = "orders.created";
        options.Queue.PrefetchCount = 16;
        options.DeadLetter.Enabled = true;
        options.DeadLetter.Exchange = "integration.dlx";
        options.DeadLetter.Queue = "integration.dlq";
        options.DeadLetter.RoutingKey = "dead-letter";
    });
```

Publish to RabbitMQ:

```csharp
public class OrderPublisher(
    IMessageEnvelopeFactory envelopeFactory,
    IMessagePublisher messagePublisher)
{
    public async Task PublishAsync(string orderId, CancellationToken cancellationToken)
    {
        var envelope = envelopeFactory.Create(
            new OrderCreatedMessage(orderId),
            destination: "orders.created",
            format: MessageFormat.Json);

        await messagePublisher.PublishAsync(envelope, cancellationToken);
    }
}
```

The package declares the configured exchange and queue on first use, binds the queue using the queue name as the routing key, and publishes messages with the envelope destination as the routing key.

Published AMQP properties include:

- `MessageId`
- `CorrelationId`
- `ContentType`
- contract/version headers from the base messaging layer
