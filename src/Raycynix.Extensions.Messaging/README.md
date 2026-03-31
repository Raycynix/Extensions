# Raycynix.Extensions.Messaging

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Messaging` contains the transport-agnostic messaging foundation for Raycynix applications.

## What it contains

- `AddRaycynixMessaging(...)`
- `MessagingBuilder`
- `MessagingConfiguration`
- `JsonMessagingConfiguration`
- `GrpcMessagingConfiguration`
- `IMessageSerializer`
- `IMessageEnvelopeFactory`
- `IRequestEnvelopeFactory`
- `IDirectRequestClient`
- `IMessageCodec`
- `IMessageCodecResolver`
- `IMessagePublisher`
- `IMessageHandler<TMessage>`
- `MessageEnvelope<TMessage>`
- `RequestEnvelope<TRequest>`
- `ResponseEnvelope<TResponse>`
- `SerializedMessage`
- built-in JSON serialization through `Newtonsoft.Json`
- delegate-based gRPC/protobuf codec registration through `AddGrpcMessage<TMessage>(...)`
- transport-neutral message envelope creation and serialization
- transport-neutral direct request/response abstractions

## What it does not contain

- broker-specific Kafka client setup
- broker-specific RabbitMQ client setup
- background consumers
- hosted services for message polling
- outbox persistence
- dead-letter queue processing

## Usage

Register the base package and optional codecs:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration, options =>
{
    options.DefaultFormat = MessageFormat.Json;
})
.AddGrpcMessage<MyGrpcMessage>(
    message => message.ToByteArray(),
    payload => MyGrpcMessage.Parser.ParseFrom(payload.Span));
```

Create and publish a message through a broker transport:

```csharp
public class OrderService(
    IMessageEnvelopeFactory envelopeFactory,
    IMessagePublisher messagePublisher)
{
    public async Task PublishOrderCreatedAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var envelope = envelopeFactory.Create(
            message,
            destination: "orders.created",
            format: MessageFormat.Json);

        await messagePublisher.PublishAsync(envelope, cancellationToken);
    }
}
```

Create and send a direct request through `HttpJson` or `Grpc` transport:

```csharp
public class CatalogGateway(
    IRequestEnvelopeFactory envelopeFactory,
    IDirectRequestClient directRequestClient)
{
    public async Task<CatalogItemResponse> GetAsync(string sku, CancellationToken cancellationToken)
    {
        var request = envelopeFactory.Create(
            new CatalogItemRequest(sku),
            destination: "catalog/get-item",
            format: MessageFormat.Json);

        var response = await directRequestClient.SendAsync<CatalogItemRequest, CatalogItemResponse>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```

Register and dispatch incoming handlers:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddMessageHandler<OrderCreatedMessage, OrderCreatedHandler>();

public sealed class OrderConsumer(IMessageDispatcher dispatcher)
{
    public async Task ConsumeAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var envelope = new MessageEnvelope<OrderCreatedMessage>
        {
            Message = message,
            Destination = "orders.created",
            Format = MessageFormat.Json,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await dispatcher.DispatchAsync(envelope, cancellationToken);
    }
}
```

Contract metadata and propagation headers are added automatically:

- `X-Contract-Name`
- `X-Contract-Version`
- `X-Correlation-Id`
- `traceparent`
- service identity headers when `ISecurityContext` is available

For a concrete transport, add one of the provider packages:

- `Raycynix.Extensions.Messaging.Kafka`
- `Raycynix.Extensions.Messaging.RabbitMQ`
- `Raycynix.Extensions.Messaging.HttpJson`
- `Raycynix.Extensions.Messaging.Grpc`
