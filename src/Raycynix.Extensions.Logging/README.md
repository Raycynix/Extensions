# Raycynix.Extensions.Logging

`Raycynix.Extensions.Logging` provides structured logging services, typed logger adapters, and Serilog host integration for Raycynix applications.

## What it contains

- `AddRaycynixLogging(...)`
- `AddRaycynixLogging()`
- `UseRaycynixLogging(this IHostBuilder, ...)`
- `LoggingBuilder`
- `LoggingConfiguration`
- Serilog-backed `ILogger<T>` implementation
- Console sink setup
- Serilog integration hooks for optional packages

## What it does not contain

- Elasticsearch sink integration
- Provider-specific log shipping packages
- ASP.NET Core middleware

Use `Raycynix.Extensions.Logging.Elastic` when logs should be written to Elasticsearch.

## appsettings.json

```json
{
  "LoggingConfiguration": {
    "ServiceName": "orders-api",
    "ServiceVersion": "1.0.0",
    "Environment": "Production",
    "MinimumLevel": "Information",
    "OutputTemplate": "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] {Message:lj}{NewLine}{Exception}"
  }
}
```

## Usage

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging()
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixLogging(context.Configuration);
        services.AddHostedService<AppWorker>();
    });
```

For default settings without configuration binding:

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging()
    .ConfigureServices(services =>
    {
        services.AddRaycynixLogging();
    });
```

Runtime overrides are applied to the same `LoggingConfiguration` instance that optional integrations receive:

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging(options =>
    {
        options.ServiceName = "orders-worker";
        options.MinimumLevel = LogLevel.Debug;
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixLogging(context.Configuration);
    });
```

## Optional integrations

Optional sinks are added through `LoggingBuilder`. When the integration needs its own configuration section, use the configuration overload:

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging()
    .ConfigureServices((context, services) =>
    {
        services
            .AddRaycynixLogging(context.Configuration)
            .AddElastic();
    });
```

`UseRaycynixLogging()` must still be called on the host builder because it connects Serilog to the generic host.
If `AddRaycynixLogging(...)` is not registered, `UseRaycynixLogging()` falls back to the host `LoggingConfiguration` section and default values.

## Injecting the typed logger

```csharp
public sealed class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public void Process(string orderId)
    {
        logger.Information("Processing order {OrderId}", orderId);
    }
}
```

`AddRaycynixLogging(context.Configuration)` also registers `LoggingConfiguration` through the Raycynix configuration pipeline and validates it on startup.
