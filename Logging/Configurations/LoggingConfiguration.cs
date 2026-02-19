using Serilog.Events;
using Raycynix.Extensions.Core.Utils;

namespace Raycynix.Extensions.Logging.Configurations
{
    /// <summary>
    /// Represents configuration options for the Raycynix logging module.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// The name of the current service or application emitting logs.
        /// </summary>
        public string ServiceName { get; } = AssemblyHelper.CurrentName();

        /// <summary>
        /// The version of the current service or application emitting logs.
        /// </summary>
        public string ServiceVersion { get; } = AssemblyHelper.CurrentVersion();

        /// <summary>
        /// The environment name (e.g., Development, Production).
        /// </summary>
        public string Environment { get; } = EnvironmentHelper.CurrentEnvironment();

        /// <value>
        /// Indicating whether Elasticsearch logging is <b>enabled</b>
        /// </value>
        public bool UseElastic { get; set; } = false;

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