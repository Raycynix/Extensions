using Messaging.Abstractions;
using Newtonsoft.Json;

namespace Messaging.Implementation.Core
{
    /// <summary>
    /// Provides JSON-based message serialization and deserialization
    /// using <see cref="Newtonsoft.Json"/> for Raycynix Messaging.
    /// </summary>
    /// <remarks>
    /// This serializer supports polymorphic types by enabling
    /// <see cref="TypeNameHandling.Auto"/>, which allows messages of derived
    /// types to be deserialized correctly when handled by generic message
    /// handlers.  
    /// It is lightweight, production-safe, and optimized for use in
    /// distributed microservice messaging environments.
    /// </remarks>
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMessageSerializer"/> class.
        /// </summary>
        /// <remarks>
        /// The serializer is configured with non-formatted output,
        /// automatic type name handling for polymorphism,
        /// and omission of null and default values to minimize payload size.
        /// </remarks>
        public JsonMessageSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        /// <inheritdoc/>
        /// <summary>
        /// Serializes the specified message into its JSON string representation.
        /// </summary>
        /// <typeparam name="T">The type of the message to serialize.</typeparam>
        /// <param name="message">The message instance to serialize.</param>
        /// <returns>A JSON string representing the message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
        public string Serialize<T>(T message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return JsonConvert.SerializeObject(message, _settings);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Deserializes the provided JSON payload into a message object.
        /// </summary>
        /// <typeparam name="T">The type of the message to deserialize into.</typeparam>
        /// <param name="payload">The JSON string to deserialize.</param>
        /// <returns>
        /// A deserialized message instance, or <c>null</c> if
        /// <paramref name="payload"/> is null or empty.
        /// </returns>
        public T? Deserialize<T>(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return default;

            return JsonConvert.DeserializeObject<T>(payload, _settings);
        }
    }
}
