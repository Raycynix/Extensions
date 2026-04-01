using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Database.Tests.Registration;

/// <summary>
/// Covers database-backed messaging, persistence registration, and runtime behavior.
/// </summary>
public sealed class MessagingDatabaseRegistrationTests
{
    /// <summary>
    /// Verifies that database-backed messaging persistence survives service-provider recreation for outbox entries.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldPersistOutboxEntriesAcrossProviders()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(TestContext.Current.CancellationToken);
                var store = provider.GetRequiredService<IMessageOutboxStore>();
                await store.EnqueueAsync(CreateSerializedMessage("msg-1"), TestContext.Current.CancellationToken);
            }

            await using var reloadedProvider = BuildProvider(databasePath);
            var reloadedStore = reloadedProvider.GetRequiredService<IMessageOutboxStore>();
            var entry = await reloadedStore.GetAsync("msg-1", TestContext.Current.CancellationToken);

            entry.Should().NotBeNull();
            entry.Status.Should().Be(MessageOutboxStatus.Pending);
            entry.Message.Destination.Should().Be("orders.created");
            entry.Message.Payload.Should().Equal(System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-1\"}"));
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that the database-backed inbox state prevents duplicate processing across service-provider recreation.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldPersistInboxStateAcrossProviders()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(TestContext.Current.CancellationToken);
                var state = provider.GetRequiredService<InboxHandlerState>();
                var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
                await processor.ProcessAsync(CreateIncomingMessage("msg-2"), TestContext.Current.CancellationToken);
                state.Values.Should().ContainSingle().Which.Should().Be("once");
            }

            await using (var provider = BuildProvider(databasePath))
            {
                var state = provider.GetRequiredService<InboxHandlerState>();
                var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
                await processor.ProcessAsync(CreateIncomingMessage("msg-2"), TestContext.Current.CancellationToken);
                state.Values.Should().BeEmpty();
            }
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that outbox failure state is recovered from persistent storage.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldRecoverFailedOutboxEntries()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(TestContext.Current.CancellationToken);
                var store = provider.GetRequiredService<IMessageOutboxStore>();
                var processorType = typeof(Messaging)
                    .Assembly
                    .GetTypes()
                    .Single(static type => type.Name == nameof(Raycynix.Extensions.Messaging.Implementations.MessageOutboxRecoveryProcessor));
                var processor = provider.GetRequiredService(processorType);

                await store.EnqueueAsync(CreateSerializedMessage("msg-3"), TestContext.Current.CancellationToken);
                await store.MarkFailedAsync(
                    "msg-3",
                    new InvalidOperationException("publish failed"),
                    DateTimeOffset.UtcNow.AddMinutes(-1),
                    TestContext.Current.CancellationToken);

                var processAvailableAsync = processor.GetType().GetMethod("ProcessAvailableAsync")!;
                var result = await (Task<int>)processAvailableAsync.Invoke(processor, [TestContext.Current.CancellationToken])!;
                result.Should().Be(1);
            }

            await using var reloadedProvider = BuildProvider(databasePath);
            var storeAfterRecovery = reloadedProvider.GetRequiredService<IMessageOutboxStore>();
            var entryAfterRecovery = await storeAfterRecovery.GetAsync("msg-3", TestContext.Current.CancellationToken);

            entryAfterRecovery.Should().NotBeNull();
            entryAfterRecovery.Status.Should().Be(MessageOutboxStatus.Dispatched);
            entryAfterRecovery.AttemptCount.Should().Be(2);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    private static ServiceProvider BuildProvider(string databasePath)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton<InboxHandlerState>();
        services.AddSingleton<ITransportMessagePublisher, RecordingTransportPublisher>();

        services.AddRaycynixDatabase(BuildDatabaseConfiguration(databasePath));
        services.AddRaycynixMessaging(BuildMessagingConfiguration())
            .AddMessageHandler<PersistedInboxMessage, PersistedInboxHandler>()
            .AddDatabasePersistence();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static IConfiguration BuildDatabaseConfiguration(string databasePath)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:ConnectionString"] = $"Data Source={databasePath}",
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:UseMigrations"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();
    }

    private static IConfiguration BuildMessagingConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MessagingConfiguration:Outbox:Enabled"] = "true",
                ["MessagingConfiguration:Outbox:EnableRecovery"] = "true",
                ["MessagingConfiguration:Outbox:AutoDispatchOnPublish"] = "false"
            })
            .Build();
    }

    private static SerializedMessage CreateSerializedMessage(string messageId)
    {
        return new SerializedMessage
        {
            MessageId = messageId,
            Destination = "orders.created",
            Payload = "{\"orderId\":\"order-1\"}"u8.ToArray(),
            Format = MessageFormat.Json,
            ContentType = "application/json",
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Format"] = nameof(MessageFormat.Json)
            }
        };
    }

    private static IncomingTransportMessage CreateIncomingMessage(string messageId)
    {
        return new IncomingTransportMessage
        {
            MessageId = messageId,
            Destination = "orders.persisted",
            Payload = "{\"value\":\"once\"}"u8.ToArray(),
            Format = MessageFormat.Json,
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Format"] = nameof(MessageFormat.Json)
            }
        };
    }

    private static void TryDelete(string databasePath)
    {
        try
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [MessageContract("orders.persisted", "1.0.0")]
    private sealed record PersistedInboxMessage(string Value);

    private sealed class InboxHandlerState
    {
        public List<string> Values { get; } = [];
    }

    private sealed class PersistedInboxHandler(InboxHandlerState state) : IMessageHandler<PersistedInboxMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<PersistedInboxMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.Values.Add(envelope.Message.Value);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingTransportPublisher : ITransportMessagePublisher
    {
        public List<string> PublishedMessageIds { get; } = [];

        public ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
        {
            PublishedMessageIds.Add(message.MessageId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, string message, Exception? exception = null, object? metadata = null)
        {
        }
    }
}
