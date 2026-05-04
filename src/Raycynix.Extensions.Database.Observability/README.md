# Raycynix.Extensions.Database.Observability

`Raycynix.Extensions.Database.Observability` adds optional tracing and metrics integration for the Raycynix database infrastructure.

## What It Contains

- `AddObservability()`
- `IDatabaseObservability` implementation backed by Raycynix tracing and metrics abstractions
- metrics for database infrastructure operation counts and durations
- tracing tags for database provider and operation names

## Usage

Register the core database package, exactly one provider package, and then enable observability:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddObservability();
```

Without this package, `Raycynix.Extensions.Database` uses a no-op observability implementation.

## Emitted Operations

The package observes database infrastructure operations such as initialization, database creation, migrations, and EF Core model creation.

Metrics use the `raycynix_database_*` prefix and include provider, operation, and status labels where applicable.
