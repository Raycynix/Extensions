# Raycynix.Extensions.Serilog.Elastic

`Raycynix.Extensions.Serilog.Elastic` integrates the officially supported
Elastic Serilog sink with `Raycynix.Extensions.Serilog`.

The package writes ECS-compatible log documents to Elasticsearch data streams.

## Requirements

- `Raycynix.Extensions.Serilog`
- `Elastic.Serilog.Sinks` 9.0.0
- Elastic Stack 8.15.0 or later

## Registration

```csharp
using Raycynix.Extensions.Serilog;
using Raycynix.Extensions.Serilog.Elastic;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRaycynixSerilog(logging =>
{
    logging.AddElastic();
});
```

## Elasticsearch configuration

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "Enabled": true,
        "ConnectionMode": "Elasticsearch",
        "Nodes": [
          "https://localhost:9200"
        ],
        "BootstrapMethod": "Silent",

        "Authentication": {
          "Mode": "ApiKey",
          "ApiKey": "replace-from-environment"
        },

        "DataStream": {
          "Type": "logs",
          "Dataset": "orders",
          "Namespace": "production"
        }
      }
    }
  }
}
```

## Elastic Cloud configuration

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "ConnectionMode": "ElasticCloud",
        "CloudId": "deployment:encoded-value",

        "Authentication": {
          "Mode": "ApiKey",
          "ApiKey": "replace-from-environment"
        }
      }
    }
  }
}
```

## Default data stream

When dataset and namespace are omitted, they are generated from the resolved
Raycynix service name and environment.

For example:

```text
ServiceName: Orders.Api
Environment: Production
```

produces:

```text
logs-orders_api-production
```

## Sink-specific minimum level

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "MinimumLevel": "Warning"
      }
    }
  }
}
```

This only filters the Elastic sink. It does not change the global Serilog level.

## Channel configuration

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "Buffer": {
          "ExportMaxRetries": 3,
          "ExportMaxConcurrency": 4,
          "InboundBufferMaxSize": 10000,
          "OutboundBufferMaxSize": 1000,
          "OutboundBufferMaxLifetime": "00:00:05",
          "FullMode": "Wait"
        }
      }
    }
  }
}
```

Buffer properties are optional. Omitted properties preserve the official sink
defaults.

## Secrets

API keys and passwords should be supplied using environment variables,
user secrets, Vault, Kubernetes Secrets, or another secret provider.

Example environment variable:

```text
Raycynix__Serilog__Elastic__Authentication__ApiKey
```