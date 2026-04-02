using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Database.Models;

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

    /// <summary>
    /// Verifies that concurrent duplicate inbox inserts do not surface primary-key failures.
    /// </summary>
    [Fact]
    public async Task TryBeginProcessingAsync_WithConcurrentDuplicateDelivery_ShouldReturnFalseInsteadOfThrowing()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(TestContext.Current.CancellationToken);
            var store = provider.GetRequiredService<IIncomingMessageInboxStore>();
            var message = CreateIncomingMessage("msg-concurrent");
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var attempts = Enumerable.Range(0, 8)
                .Select(async _ =>
                {
                    await start.Task.ConfigureAwait(false);
                    return await store.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);
                })
                .ToArray();

            start.SetResult();
            var results = await Task.WhenAll(attempts);

            results.Count(static current => current).Should().Be(1);
            results.Count(static current => !current).Should().Be(7);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that stale Processing inbox entries can be reclaimed after an interrupted consumer run.
    /// </summary>
    [Fact]
    public async Task TryBeginProcessingAsync_WithStaleProcessingEntry_ShouldReclaimLease()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(
                databasePath,
                new Dictionary<string, string?>
                {
                    ["MessagingConfiguration:IncomingProcessing:ProcessingLeaseTimeout"] = "00:00:01"
                });
            await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(TestContext.Current.CancellationToken);

            var store = provider.GetRequiredService<IIncomingMessageInboxStore>();
            var message = CreateIncomingMessage("msg-stale");
            var firstAttempt = await store.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);

            firstAttempt.Should().BeTrue();

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var entry = await databaseContext.Set<MessagingInboxEntryEntity>()
                    .SingleAsync(current => current.MessageId == message.MessageId, TestContext.Current.CancellationToken);
                entry.UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var reclaimed = await store.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);

            reclaimed.Should().BeTrue();
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    private static ServiceProvider BuildProvider(
        string databasePath,
        params IEnumerable<KeyValuePair<string, string?>>[] additionalConfiguration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton<InboxHandlerState>();
        services.AddSingleton<ITransportMessagePublisher, RecordingTransportPublisher>();

        services.AddRaycynixDatabase(BuildDatabaseConfiguration(databasePath));
        services.AddRaycynixMessaging(BuildMessagingConfiguration(additionalConfiguration))
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

    private static IConfiguration BuildMessagingConfiguration(
        params IEnumerable<KeyValuePair<string, string?>>[] additionalConfiguration)
    {
        var values = new Dictionary<string, string?>
        {
            ["MessagingConfiguration:Outbox:Enabled"] = "true",
            ["MessagingConfiguration:Outbox:EnableRecovery"] = "true",
            ["MessagingConfiguration:Outbox:AutoDispatchOnPublish"] = "false"
        };

        foreach (var pair in additionalConfiguration.SelectMany(static current => current))
        {
            values[pair.Key] = pair.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
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
