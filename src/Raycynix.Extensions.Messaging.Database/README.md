# Raycynix.Extensions.Messaging.Database

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Messaging.Database` adds persistent inbox and outbox storage for Raycynix messaging on top of `Raycynix.Extensions.Database`.

## What it contains

- `AddDatabasePersistence(...)`
- persistent `IIncomingMessageInboxStore`
- persistent `IMessageOutboxStore`
- EF Core configurators registered into the shared `DatabaseContext`
- ambient unit-of-work aware outbox persistence for shared `DatabaseContext` scopes
- optional background retention cleanup for inbox/outbox tables

## What it does not contain

- broker-specific Kafka client setup
- broker-specific RabbitMQ client setup
- EF Core entities for your application domain
- cross-resource distributed transactions

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql();

builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddDatabasePersistence(options =>
    {
        options.InboxTableName = "messaging_inbox";
        options.OutboxTableName = "messaging_outbox";
        options.ProcessedInboxRetention = TimeSpan.FromDays(3);
        options.DispatchedOutboxRetention = TimeSpan.FromDays(3);
    });
```

The package replaces the default in-memory inbox/outbox stores with database-backed implementations and registers its EF Core configurators into the shared `DatabaseContext` through `AddRaycynixDatabaseAssembly(...)`. Table creation still flows through the existing Raycynix database initialization pipeline.

Inbox and outbox lease acquisition uses optimistic concurrency through EF Core model metadata, so the package stays provider-agnostic across SQLite, PostgreSQL, SQL Server, and MySQL without introducing provider-specific SQL into the messaging layer.

This package gives messaging persistence that survives process restarts, participates in the ambient shared `DatabaseContext` unit of work for outbox writes, runs retention cleanup, and works with the existing outbox recovery pipeline. It does not provide distributed transactions, but it does provide durable inbox/outbox state and database-backed recovery and dispatch leasing in the configured relational database.
