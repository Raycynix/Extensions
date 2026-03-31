using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Serialization;

namespace Raycynix.Extensions.Messaging;

/// <summary>
/// Provides fluent registration for Raycynix messaging services.
/// </summary>
public sealed class MessagingBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Registers a custom payload codec.
    /// </summary>
    /// <typeparam name="TCodec">The codec implementation type.</typeparam>
    /// <returns>The same builder instance.</returns>
    public MessagingBuilder AddCodec<TCodec>()
        where TCodec : class, IMessageCodec
    {
        Services.AddSingleton<IMessageCodec, TCodec>();
        return this;
    }

    /// <summary>
    /// Registers a message handler for incoming dispatch operations.
    /// </summary>
    /// <typeparam name="TMessage">The payload type handled by the implementation.</typeparam>
    /// <typeparam name="THandler">The handler implementation type.</typeparam>
    /// <returns>The same builder instance.</returns>
    public MessagingBuilder AddMessageHandler<TMessage, THandler>()
        where THandler : class, IMessageHandler<TMessage>
    {
        Services.AddScoped<IMessageHandler<TMessage>, THandler>();
        return this;
    }

    /// <summary>
    /// Registers delegate-based gRPC/protobuf serialization for a specific message type.
    /// </summary>
    /// <typeparam name="TMessage">The payload type.</typeparam>
    /// <param name="serialize">The delegate that serializes the payload.</param>
    /// <param name="deserialize">The delegate that deserializes the payload.</param>
    /// <param name="contentType">An optional content type override.</param>
    /// <returns>The same builder instance.</returns>
    public MessagingBuilder AddGrpcMessage<TMessage>(
        Func<TMessage, byte[]> serialize,
        Func<ReadOnlyMemory<byte>, TMessage> deserialize,
        string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(serialize);
        ArgumentNullException.ThrowIfNull(deserialize);

        Services.AddSingleton<IMessageCodec>(
            new GrpcMessageCodec<TMessage>(serialize, deserialize, contentType));

        return this;
    }
}
