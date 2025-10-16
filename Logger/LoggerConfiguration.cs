using Serilog.Events;

namespace Extensions.Logging
{
    public class LoggerConfiguration
    {
        public string ServiceName { get; set; } = "Microservice";
        public string ServiceVersion { get; set; } = "0.0.1";
        public string ElasticUrl { get; set; } = "http://localhost:9200";

        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    }
}
