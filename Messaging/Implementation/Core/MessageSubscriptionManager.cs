using Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Implementation.Core
{
    /// <summary>
    /// Manages subscriptions between message types and their handlers.
    /// </summary>
    public class MessageSubscriptionManager(IServiceProvider services)
    {
        private readonly Dictionary<string, Type> _handlers = [];

        /// <summary>
        /// Registers a message type and its handler.
        /// </summary>
        public void Register<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>
        {
            _handlers[typeof(TMessage).FullName!] = typeof(THandler);
        }

        /// <summary>
        /// Invokes the handler for the given message type.
        /// </summary>
        public async Task HandleAsync(string typeName, string payload, IMessageSerializer serializer, CancellationToken token = default)
        {
            if (!_handlers.TryGetValue(typeName, out var handlerType))
                return;

            using var scope = services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);

            var messageType = Type.GetType(typeName)!;
            var message = serializer.Deserialize(payload, messageType);

            var method = handlerType.GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, [message!, token])!;
        }
    }

    internal static class SerializerExtensions
    {
        public static object? Deserialize(this IMessageSerializer serializer, string payload, Type type)
        {
            var deserializeMethod = typeof(IMessageSerializer)
                .GetMethod(nameof(IMessageSerializer.Deserialize))!
                .MakeGenericMethod(type);

            return deserializeMethod.Invoke(serializer, [payload]);
        }
    }
}
