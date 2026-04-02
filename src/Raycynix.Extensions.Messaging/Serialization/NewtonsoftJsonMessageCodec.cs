using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Serialization;

/// <summary>
/// Provides JSON serialization using Newtonsoft.Json.
/// </summary>
public sealed class NewtonsoftJsonMessageCodec : IMessageCodec
{
    private readonly JsonSerializerSettings _serializerSettings;

    /// <summary>
    /// Initializes a new codec instance.
    /// </summary>
    /// <param name="configuration">The messaging configuration.</param>
    public NewtonsoftJsonMessageCodec(MessagingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _serializerSettings = new JsonSerializerSettings
        {
            Formatting = configuration.Json.WriteIndented ? Formatting.Indented : Formatting.None,
            ContractResolver = configuration.Json.UseCamelCase
                ? new CamelCasePropertyNamesContractResolver()
                : new DefaultContractResolver()
        };
    }

    /// <inheritdoc />
    public MessageFormat Format => MessageFormat.Json;

    /// <inheritdoc />
    public string ContentType => MessageContentTypes.Json;

    /// <inheritdoc />
    public bool CanHandle(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        return true;
    }

    /// <inheritdoc />
    public byte[] Serialize(object message, Type messageType)
    {
        ArgumentNullException.ThrowIfNull(message);
        var json = JsonConvert.SerializeObject(message, messageType, _serializerSettings);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <inheritdoc />
    public object Deserialize(ReadOnlyMemory<byte> payload, Type messageType)
    {
        var json = Encoding.UTF8.GetString(payload.Span);
        var result = JsonConvert.DeserializeObject(json, messageType, _serializerSettings);
        return result ?? throw new InvalidOperationException($"Unable to deserialize JSON payload into '{messageType.FullName}'.");
    }
}
