using Serilog.Events;

namespace Logging.Configurations
{
    /// <summary>
    /// Represents configuration options for the Raycynix logging module.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// The name of the current service or application emitting logs.
        /// </summary>
        public string ServiceName { get; set; } = "Microservice";

        /// <summary>
        /// The version of the current service or application emitting logs.
        /// </summary>
        public string ServiceVersion { get; set; } = "0.1.0";

        /// <summary>
        /// The environment name (e.g., Development, Production).
        /// </summary>
        public string Environment { get; set; } =
            System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        /// <summary>
        /// The URI of the Elasticsearch server where logs are sent.
        /// </summary>
        public string ElasticUrl { get; set; } = "http://localhost:9200";

        /// <summary>
        /// The minimum log event level to capture.
        /// </summary>
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    }
}
