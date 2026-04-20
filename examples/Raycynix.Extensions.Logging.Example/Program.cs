using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Logging;
using Raycynix.Extensions.Logging.Example;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateDefaultBuilder(args);

builder
    .UseRaycynixLogging(options =>
    {
        options.MinimumLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
        options.OutputTemplate =
            "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] {Message:lj} {NewLine}{Exception}";
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixLogging(context.Configuration);
        services.AddHostedService<LoggingExampleWorker>();
        services.AddSingleton<OrderProcessor>();
    });

await builder.RunConsoleAsync();