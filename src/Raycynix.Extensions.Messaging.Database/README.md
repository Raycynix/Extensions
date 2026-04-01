# Raycynix.Extensions.Messaging.Database

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Messaging.Database` adds persistent inbox and outbox storage for Raycynix messaging on top of `Raycynix.Extensions.Database`.

## What it contains

- `AddDatabasePersistence(...)`
- persistent `IIncomingMessageInboxStore`
- persistent `IMessageOutboxStore`
- EF Core configurators registered into the shared `DatabaseContext`

## What it does not contain

- broker-specific Kafka client setup
- broker-specific RabbitMQ client setup
- EF Core entities for your application domain
- cross-resource distributed transactions

## Usage

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration);

builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddDatabasePersistence(options =>
    {
        options.InboxTableName = "messaging_inbox";
        options.OutboxTableName = "messaging_outbox";
    });
```

The package replaces the default in-memory inbox/outbox stores with database-backed implementations and registers its EF Core configurators into the shared `DatabaseContext` through `AddRaycynixDatabaseAssembly(...)`. Table creation still flows through the existing Raycynix database initialization pipeline.

This package gives messaging persistence that survives process restarts and works with the existing outbox recovery pipeline. It does not provide distributed transactions, but it does provide durable inbox/outbox state in the configured database.
