using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Serialization;

/// <summary>
/// Provides delegate-based gRPC/protobuf serialization for a specific message type.
/// </summary>
/// <typeparam name="TMessage">The payload type.</typeparam>
public sealed class GrpcMessageCodec<TMessage> : IMessageCodec
{
    private readonly Func<ReadOnlyMemory<byte>, TMessage> _deserialize;
    private readonly Func<TMessage, byte[]> _serialize;

    /// <summary>
    /// Initializes a new codec instance.
    /// </summary>
    /// <param name="serialize">The serialization delegate.</param>
    /// <param name="deserialize">The deserialization delegate.</param>
    /// <param name="contentType">An optional content type override.</param>
    public GrpcMessageCodec(
        Func<TMessage, byte[]> serialize,
        Func<ReadOnlyMemory<byte>, TMessage> deserialize,
        string? contentType = null)
    {
        _serialize = serialize ?? throw new ArgumentNullException(nameof(serialize));
        _deserialize = deserialize ?? throw new ArgumentNullException(nameof(deserialize));
        ContentType = string.IsNullOrWhiteSpace(contentType) ? MessageContentTypes.Grpc : contentType;
    }

    /// <inheritdoc />
    public MessageFormat Format => MessageFormat.Grpc;

    /// <inheritdoc />
    public string ContentType { get; }

    /// <inheritdoc />
    public bool CanHandle(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        return typeof(TMessage).IsAssignableFrom(messageType);
    }

    /// <inheritdoc />
    public byte[] Serialize(object message, Type messageType)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _serialize((TMessage)message);
    }

    /// <inheritdoc />
    public object Deserialize(ReadOnlyMemory<byte> payload, Type messageType)
    {
        return _deserialize(payload)!;
    }
}
