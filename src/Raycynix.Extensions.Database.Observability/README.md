# Raycynix.Extensions.Database.Observability

Optional tracing and metrics integration for Raycynix database infrastructure operations.

## What It Provides

- `AddObservability()`
- an `IDatabaseObservability` implementation backed by Raycynix tracing and metrics abstractions
- operation counters with provider, operation, and status labels
- duration histograms for observed database operations
- trace tags for provider and operation names

Without this package, `Raycynix.Extensions.Database` uses a no-op observability implementation.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddObservability();
```

Register the core database package and exactly one provider before enabling observability.

## Observed Operations

The package observes infrastructure operations such as:

- database initialization
- `EnsureCreated`
- EF Core migrations
- model creation

Metrics use the `raycynix_database_*` prefix.
