using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Logging.Example;
using Raycynix.Extensions.Serilog;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateDefaultBuilder(args);

builder.UseRaycynixSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<LoggingExampleWorker>();
        services.AddSingleton<OrderProcessor>();
    });

await builder.RunConsoleAsync();
