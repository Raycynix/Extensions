# Raycynix.Extensions.Logging

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Logging` provides structured logging services and generic-host integration for Raycynix applications.

## What it contains

- `AddRaycynixLogging(...)`
- `UseRaycynixLogging(this IHostBuilder ...)`
- `LoggingConfiguration`
- Serilog-based logger implementation

## Usage

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .UseRaycynixLogging()
    .ConfigureServices(services =>
    {
        services.AddRaycynixLogging();
        services.AddHostedService<AppWorker>();
    });

await builder.RunConsoleAsync();
```

This package is not tied to ASP.NET Core middleware and can be used in web, worker, and console applications built on the generic host.
