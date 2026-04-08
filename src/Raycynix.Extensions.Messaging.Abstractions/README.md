# Raycynix.Extensions.Messaging.Abstractions

`Raycynix.Extensions.Messaging.Abstractions` contains the transport-neutral contracts used by Raycynix messaging packages.

## What it contains

- `MessageFormat`
- `MessageContentTypes`
- `MessageEnvelope<TMessage>`
- `SerializedMessage`
- `IMessageCodec`
- `IMessageCodecResolver`
- `IMessageEnvelopeFactory`
- `IMessagePublisher`
- `IMessageHandler<TMessage>`
- `IRequestClient`
- `IRequestEnvelopeFactory`
- `RequestEnvelope<TRequest>`
- `ResponseEnvelope<TResponse>`
- transport-neutral contracts for publishers, handlers, codecs, and envelopes

## What it does not contain

- DI registration
- JSON serializer implementation
- gRPC serializer implementation
- Kafka producer implementation
- RabbitMQ publisher implementation
- hosted services

## Usage

Declare a message contract:

```csharp
[MessageContract("orders.created", "1.0.0")]
public sealed record OrderCreatedMessage(string OrderId);
```

Implement a handler contract:

```csharp
public sealed class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
{
    public ValueTask HandleAsync(
        MessageEnvelope<OrderCreatedMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(envelope.Message.OrderId);
        return ValueTask.CompletedTask;
    }
}
```

Define a direct request contract:

```csharp
public sealed record GetOrderRequest(string OrderId);

public sealed record GetOrderResponse(string OrderId, string Status);
```

Use the unified incoming dispatcher contract:

```csharp
public sealed class IncomingProcessor(IMessageDispatcher dispatcher)
{
    public ValueTask<MessageDispatchResult> ProcessAsync(
        MessageEnvelope<OrderCreatedMessage> envelope,
        CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(envelope, cancellationToken);
    }
}
```

Implement a custom codec:

```csharp
public sealed class CustomBinaryCodec : IMessageCodec
{
    public MessageFormat Format => MessageFormat.Grpc;

    public string ContentType => "application/x-custom-binary";

    public bool CanHandle(Type messageType) => messageType == typeof(MyMessage);

    public byte[] Serialize(object message, Type messageType) => ((MyMessage)message).ToByteArray();

    public object Deserialize(ReadOnlyMemory<byte> payload, Type messageType) => MyMessage.Parser.ParseFrom(payload.Span);
}
```

Implement a custom direct client abstraction:

```csharp
public sealed class OrdersClient(IRequestClient requestClient)
{
    public async Task<GetOrderResponse> GetAsync(
        RequestEnvelope<GetOrderRequest> request,
        CancellationToken cancellationToken)
    {
        var response = await requestClient.SendAsync<GetOrderRequest, GetOrderResponse>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```
