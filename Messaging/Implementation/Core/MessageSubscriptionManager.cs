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
        /// Auto-discovers all IMessageHandler&lt;T&gt; implementations in loaded assemblies.
        /// </summary>
        public void AutoDiscoverHandlers()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName));

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    var interfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

                    foreach (var handlerInterface in interfaces)
                    {
                        var messageType = handlerInterface.GetGenericArguments()[0];
                        _handlers[messageType.FullName!] = type;
                    }
                }
            }
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

        /// <summary>
        /// Returns all discovered message types for automatic subscription.
        /// </summary>
        public IEnumerable<Type> GetAllMessageTypes() =>
            _handlers.Keys.Select(Type.GetType).Where(t => t is not null)!;
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
