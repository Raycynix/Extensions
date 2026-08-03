# Raycynix.Extensions.Serilog

`Raycynix.Extensions.Serilog` is a thin, opinionated integration layer over Serilog for .NET hosted applications.

It does not replace `Microsoft.Extensions.Logging` and does not implement a custom logger abstraction. Application
services continue to consume the standard `ILogger<T>` interface.

## Features

- One-line Serilog host registration
- Native `Microsoft.Extensions.Logging` integration
- Native Serilog configuration support
- Dependency-injection support for sinks and enrichers
- Default service metadata enrichment
- Default console fallback
- Extensible configurator pipeline
- Support for optional integration packages

## Registration

```csharp
using Raycynix.Extensions.Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRaycynixSerilog();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
```

ASP.NET Core uses the same extension:

```csharp
using Raycynix.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixSerilog();

var app = builder.Build();

app.MapGet("/", () => "Hello");

await app.RunAsync();
```

---

## Logging

Application services use the standard Microsoft logger:

```csharp
using Microsoft.Extensions.Logging;

public sealed class OrderProcessor(
ILogger<OrderProcessor> logger)
{
public void Process(string orderId)
{
logger.LogInformation(
"Processing order {OrderId}",
orderId);
}
}
```

---

## Raycynix configuration

```json
{
  "Raycynix": {
    "Serilog": {
      "ServiceName": "orders-api",
      "ServiceVersion": "1.0.0",
      "Environment": "Production",
      "ApplyDefaultLevelOverrides": true,
      "UseDefaultConsoleWhenNoSinksConfigured": true
    }
  }
}
```

When `ServiceName`, `ServiceVersion`, or `Environment` are omitted, the package resolves them from the host and entry
assembly.
---

## Native Serilog configuration

Sinks, levels, filters, enrichers, and destructuring remain native Serilog configuration:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```

When `Serilog:WriteTo` exists, the fallback console sink is not added.
---

## Configuration in code

```csharp
builder.AddRaycynixSerilog (logging =>
{ 
    logging.Options.ServiceName = "orders-api";
    logging.Options.ServiceVersion = "1.2.0";

    logging.ConfigureLogger((context, logger) =>
    {
        logger.Enrich.WithProperty(
            "ApplicationGroup",
            "Commerce");
    });
}); 
```

---

## Typed configurators

```csharp
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Contexts;
using Serilog;

public sealed class ApplicationSerilogConfigurator : IRaycynixSerilogConfigurator 
{ 
    public int Order => 100;

    public void Configure(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogContext context)
    {
        loggerConfiguration.Enrich.WithProperty(
            "Region",
            "eu-west");
    }
}
```

Registration:

```csharp
builder.AddRaycynixSerilog (logging =>
{
    logging.AddConfigurator<ApplicationSerilogConfigurator>();
});
```

---

## Dependency-injected Serilog components

The pipeline calls `ReadFrom.Services()`, so standard Serilog components can be registered directly in DI:

```csharp
builder.Services.AddSingleton<ILogEventEnricher, UserContextEnricher>();

builder.AddRaycynixSerilog ();
```

Supported Serilog DI components include sinks, enrichers, filters, destructuring policies, and level switches.
