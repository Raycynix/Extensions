using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Configurations;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Raycynix.Extensions.Logging.Implementation
{
    /// <summary>
    /// Represents a typed Raycynix logger implementation compatible with <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/>.
    /// </summary>
    /// <typeparam name="T">The logging category type.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Logger{T}"/> class.
    /// </remarks>
    public class Logger<T> : Abstractions.ILogger<T>
    {
        private readonly Serilog.Core.Logger _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="Logger{T}"/> class using the specified configuration.
        /// </summary>
        /// <param name="configuration">The <see cref="LoggingConfiguration"/> settings, including service details and sink options.</param>
        public Logger(LoggingConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", configuration.ServiceName)
                .Enrich.WithProperty("Version", configuration.ServiceVersion)
                .Enrich.WithProperty("Environment", configuration.Environment)
                .Enrich.WithProperty("Class", typeof(T).FullName)
                .WriteTo.Console();

            if (configuration.UseElastic)
            {
                loggerConfiguration.WriteTo.Elasticsearch([new Uri(configuration.ElasticUrl)], options =>
                {
                    options.MinimumLevel = configuration.MinimumLevel;
                    options.DataStream = new DataStreamName
                    (
                        "logs",
                        configuration.ServiceName.ToLowerInvariant(),
                        configuration.Environment.ToLowerInvariant()
                    );
                    options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
                });
            }

            _logger = loggerConfiguration.CreateLogger();
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string>? formatter)
        {
            if (formatter is null) return;

            var message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _logger.Debug("{Message}", message);
                    break;
                case LogLevel.Information:
                    _logger.Information("{Message}", message);
                    break;
                case LogLevel.Warning:
                    _logger.Warning("{Exception}: {Message}", exception, message);
                    break;
                case LogLevel.Error:
                    _logger.Error("{Exception}: {Message}", exception, message);
                    break;
                case LogLevel.Critical:
                    _logger.Fatal("{Exception}: {Message}", exception, message);
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}