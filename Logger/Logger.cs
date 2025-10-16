using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Extensions.Logging
{
    public class Logger<T> : ICustomLogger<T>
    {
        private readonly Serilog.ILogger _logger;

        public Logger(LoggerConfiguration config)
        {

            var loggerConfiguration = new Serilog.LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", config.ServiceName)
                .Enrich.WithProperty("Version", config.ServiceVersion)
                .Enrich.WithProperty("Class", typeof(T).FullName)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions()
                {
                    MinimumLevel = config.MinimumLevel,
                })
                .CreateLogger();
        }

        public IDisposable? BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
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
