# Raycynix.Extensions.Logging.Elastic

`Raycynix.Extensions.Logging.Elastic` adds Elasticsearch output to the Raycynix Serilog logging pipeline.

## What it contains

- `AddElastic(...)`
- `ElasticConfiguration`
- Serilog Elasticsearch sink configuration

## Usage

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

## appsettings.json

Elastic settings are nested under the base logging section:

```json
{
  "LoggingConfiguration": {
    "ServiceName": "orders-api",
    "ServiceVersion": "1.0.0",
    "Environment": "Production",
    "MinimumLevel": "Information",

    "ElasticConfiguration": {
      "Enabled": true,
      "Url": "http://localhost:9200"
    }
  }
}
```

`AddElastic()` binds `ElasticConfiguration` from `LoggingConfiguration:ElasticConfiguration` and registers a Serilog configurator. `UseRaycynixLogging()` applies that configurator when the host builds the Serilog logger.

If logging was registered without configuration, pass configuration directly to Elastic:

```csharp
services
    .AddRaycynixLogging()
    .AddElastic(context.Configuration);
```

You can override Elastic settings in code:

```csharp
services
    .AddRaycynixLogging(context.Configuration)
    .AddElastic(options =>
    {
        options.Enabled = true;
        options.Url = "http://elastic:9200";
    });
```
