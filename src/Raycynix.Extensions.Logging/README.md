# Raycynix.Extensions.Logging

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Logging` provides structured logging services and generic-host integration for Raycynix applications.

## What it contains

- `AddRaycynixLogging(...)`
- `AddRaycynixLogging(IConfiguration, ...)`
- `UseRaycynixLogging(this IHostBuilder ...)`
- `LoggingConfiguration`
- Serilog-based logger implementation

## appsettings.json

```json
{
  "LoggingConfiguration": {
    "ServiceName": "orders-api",
    "ServiceVersion": "1.0.0",
    "Environment": "Production",
    "MinimumLevel": "Information",
    "UseElastic": false,
    "ElasticUrl": "http://localhost:9200",
    "OutputTemplate": "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] {Message:lj}{NewLine}{Exception}"
  }
}
```

## Usage

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging()
    .ConfigureServices(services =>
    {
        services.AddRaycynixLogging(builder.Configuration);
        services.AddHostedService<AppWorker>();
    });

await builder.RunConsoleAsync();
```

You can also override bound settings in code:

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging(options =>
    {
        options.ServiceName = "orders-worker";
        options.MinimumLevel = LogLevel.Debug;
        options.UseElastic = true;
        options.ElasticUrl = "http://elastic:9200";
    })
    .ConfigureServices(services =>
    {
        services.AddRaycynixLogging();
    });
```

## Injecting the typed logger

```csharp
public sealed class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public void Process(string orderId)
    {
        logger.Information("Processing order", new { OrderId = orderId });
    }
}
```

`AddRaycynixLogging(builder.Configuration)` also registers `LoggingConfiguration` through the standard Raycynix configuration pipeline and validates it on startup.

This package is not tied to ASP.NET Core middleware and can be used in web, worker, and console applications built on the generic host.
