using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Logging.Abstractions;
using Logging.Configurations;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Logging.Implementation
{
    /// <summary>
    /// Represents a typed Raycynix logger implementation compatible with <see cref="ILogger{TCategoryName}"/>.
    /// </summary>
    /// <typeparam name="T">The logging category type.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Logger{T}"/> class.
    /// </remarks>
    /// <param name="configuration">The logger configuration.</param>
    public class Logger<T>(LoggingConfiguration configuration) : ICustomLogger<T>
    {
        private readonly Serilog.Core.Logger _logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", configuration.ServiceName)
                .Enrich.WithProperty("Version", configuration.ServiceVersion)
                .Enrich.WithProperty("Environment", configuration.Environment)
                .Enrich.WithProperty("Class", typeof(T).FullName)
                .WriteTo.Console()
                .WriteTo.Elasticsearch([new Uri(configuration.ElasticUrl)], options =>
                {
                    options.MinimumLevel = options.MinimumLevel;
                    options.DataStream = new DataStreamName
                    (
                        "logs",
                        configuration.ServiceName.ToLowerInvariant(),
                        configuration.Environment.ToLowerInvariant()
                    );
                    options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
                })
                .CreateLogger();

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
        {
            if (formatter is null) return;

            var message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _logger.Debug(message);
                    break;
                case LogLevel.Information:
                    _logger.Information(message);
                    break;
                case LogLevel.Warning:
                    _logger.Warning(exception, message);
                    break;
                case LogLevel.Error:
                    _logger.Error(exception, message);
                    break;
                case LogLevel.Critical:
                    _logger.Fatal(exception, message);
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}
