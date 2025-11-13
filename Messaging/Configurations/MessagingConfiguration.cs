namespace Messaging.Configurations
{
    /// <summary>
    /// Defines configuration options for the unified Raycynix message bus.
    /// </summary>
    public class MessagingConfiguration
    {
        public bool EnableRabbitMq { get; set; } = true;
        public bool EnableKafka { get; set; } = false;

        public RabbitMqOptions RabbitMq { get; set; } = new();
        public KafkaOptions Kafka { get; set; } = new();
    }
    public class RabbitMqOptions
    {
        public string Host { get; set; } = "localhost";
        public string User { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "raycynix";
    }

    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "raycynix-group";
    }
}
