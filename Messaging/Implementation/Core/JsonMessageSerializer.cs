using Messaging.Abstractions;
using Newtonsoft.Json;

namespace Messaging.Implementation.Core
{
    /// <summary>
    /// Provides JSON-based message serialization and deserialization
    /// using <see cref="Newtonsoft.Json"/> for Raycynix Messaging.
    /// </summary>
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _settings;

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
        public string Serialize<T>(T message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return JsonConvert.SerializeObject(message, _settings);
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return default;

            return JsonConvert.DeserializeObject<T>(payload, _settings);
        }
    }
}
