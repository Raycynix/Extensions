namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines a contract for serializing and deserializing messages.
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serializes a message object into a string.
        /// </summary>
        string Serialize<T>(T message);

        /// <summary>
        /// Deserializes a string payload into a message object.
        /// </summary>
        T? Deserialize<T>(string payload);
    }
}
