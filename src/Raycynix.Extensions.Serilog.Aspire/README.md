# Raycynix.Extensions.Serilog.Aspire

`Raycynix.Extensions.Serilog.Aspire` adds direct Raycynix Serilog registration
for an Aspire AppHost process.

## AppHost registration

```csharp
using Raycynix.Extensions.Serilog.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddRaycynixSerilog();

builder.AddProject<Projects.Api>("api");

builder.Build().Run();
```

The call configures logging for the AppHost process. Aspire resources run in
separate processes, so each .NET service must also reference
`Raycynix.Extensions.Serilog` and call `AddRaycynixSerilog()` on its own
`WebApplicationBuilder` or `HostApplicationBuilder`.

The same configuration sections used by the core package apply to AppHost:

```json
{
  "Raycynix": {
    "Serilog": {
      "ServiceName": "commerce-apphost"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```
