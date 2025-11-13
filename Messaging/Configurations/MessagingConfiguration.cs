namespace Messaging.Configurations
{
    /// <summary>
    /// Represents the unified configuration model for the Raycynix Messaging subsystem.
    /// </summary>
    /// <remarks>
    /// This configuration defines connection and broker settings
    /// for supported message brokers such as RabbitMQ and Kafka.
    /// The configuration is usually bound from <c>appsettings.json</c> via
    /// <c>IConfiguration</c> in the service registration pipeline.
    /// </remarks>
    /// <example>
    /// Example configuration in <c>appsettings.json</c>:
    /// <code>
    /// {
    ///   "MessagingConfiguration": {
    ///     "EnableRabbitMq": true,
    ///     "EnableKafka": false,
    ///     "RabbitMq": {
    ///       "Host": "localhost",
    ///       "User": "guest",
    ///       "Password": "guest",
    ///       "Exchange": "raycynix"
    ///     },
    ///     "Kafka": {
    ///       "BootstrapServers": "localhost:9092",
    ///       "GroupId": "raycynix-group"
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public class MessagingConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether RabbitMQ messaging should be enabled.
        /// </summary>
        /// <value><c>true</c> to enable RabbitMQ integration; otherwise, <c>false</c>.</value>
        public bool EnableRabbitMq { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Kafka messaging should be enabled.
        /// </summary>
        /// <value><c>true</c> to enable Kafka integration; otherwise, <c>false</c>.</value>
        public bool EnableKafka { get; set; } = false;

        /// <summary>
        /// Gets or sets RabbitMQ-specific configuration options.
        /// </summary>
        /// <value>An instance of <see cref="RabbitMqOptions"/> that describes RabbitMQ connection settings.</value>
        public RabbitMqOptions RabbitMq { get; set; } = new();

        /// <summary>
        /// Gets or sets Kafka-specific configuration options.
        /// </summary>
        /// <value>An instance of <see cref="KafkaOptions"/> that describes Kafka connection settings.</value>
        public KafkaOptions Kafka { get; set; } = new();
    }

    /// <summary>
    /// Represents configuration settings for connecting to a RabbitMQ broker.
    /// </summary>
    /// <remarks>
    /// These options are bound from the <c>Messaging:RabbitMq</c> section in configuration files.
    /// </remarks>
    public class RabbitMqOptions
    {
        /// <summary>
        /// Gets or sets the hostname or IP address of the RabbitMQ server.
        /// </summary>
        /// <value>The RabbitMQ host address. Default is <c>localhost</c>.</value>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the username for RabbitMQ authentication.
        /// </summary>
        /// <value>The login username. Default is <c>guest</c>.</value>
        public string User { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the password for RabbitMQ authentication.
        /// </summary>
        /// <value>The login password. Default is <c>guest</c>.</value>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Gets or sets the default exchange name to use when publishing or subscribing to topics.
        /// </summary>
        /// <value>The exchange name. Default is <c>raycynix</c>.</value>
        public string Exchange { get; set; } = "raycynix";
    }

    /// <summary>
    /// Represents configuration settings for connecting to a Kafka broker.
    /// </summary>
    /// <remarks>
    /// These options are bound from the <c>Messaging:Kafka</c> section in configuration files.
    /// </remarks>
    public class KafkaOptions
    {
        /// <summary>
        /// Gets or sets the comma-separated list of Kafka bootstrap servers.
        /// </summary>
        /// <value>The Kafka bootstrap servers. Default is <c>localhost:9092</c>.</value>
        public string BootstrapServers { get; set; } = "localhost:9092";

        /// <summary>
        /// Gets or sets the consumer group identifier for Kafka subscriptions.
        /// </summary>
        /// <value>The Kafka consumer group ID. Default is <c>raycynix-group</c>.</value>
        public string GroupId { get; set; } = "raycynix-group";
    }
}
