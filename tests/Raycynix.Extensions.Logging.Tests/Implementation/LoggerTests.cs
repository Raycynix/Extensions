using FluentAssertions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Raycynix.Extensions.Logging.Tests.Implementation;

/// <summary>
/// Covers the typed logger adapter built on top of Serilog.
/// </summary>
public sealed class LoggerTests
{
    /// <summary>
    /// Verifies that log entries preserve the message, metadata, and mapped severity.
    /// </summary>
    [Fact]
    public void Log_ShouldWriteStructuredEvent_WithMappedLevelAndMetadata()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);

        logger.Log(LogLevel.Warning, "Price recalculated", metadata: new { ProductId = "sku-1" });

        sink.Events.Should().ContainSingle();
        var entry = sink.Events.Single();
        entry.Level.Should().Be(LogEventLevel.Warning);
        entry.Properties["Message"].ToString().Should().Be("\"Price recalculated\"");
        entry.Properties["Metadata"].ToString().Should().Contain("sku-1");
    }

    /// <summary>
    /// Verifies that exception logging includes the supplied exception instance.
    /// </summary>
    [Fact]
    public void Log_ShouldAttachException_WhenProvided()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);
        var exception = new InvalidOperationException("boom");
        
        logger.Error("Failure", exception, new { Operation = "checkout" });

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Exception.Should().BeSameAs(exception);
    }

    /// <summary>
    /// Verifies that the Microsoft logger interface path uses the formatter result and state as metadata.
    /// </summary>
    [Fact]
    public void MicrosoftLoggerLog_ShouldUseFormatterOutput_AndStateMetadata()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        ILogger<TestCategory> logger =
            new Implementations.Logger<TestCategory>(serilog);
        var state = new Dictionary<string, object?> { ["ProductId"] = "sku-1" };

        logger.Log(LogLevel.Information, new EventId(42, "PriceRead"), state, null, static (current, _) => $"Read {current["ProductId"]}");

        sink.Events.Should().ContainSingle();
        var entry = sink.Events.Single();
        entry.Properties["Message"].ToString().Should().Be("\"Read sku-1\"");
        entry.Properties["Metadata"].ToString().Should().Contain("sku-1");
    }

    /// <summary>
    /// Verifies that scopes are emitted when the underlying Serilog logger is enriched from log context.
    /// </summary>
    [Fact]
    public void BeginScope_ShouldPushScopeIntoSubsequentEvents()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);

        using (logger.BeginScope(new { CorrelationId = "corr-1" }))
        {
            logger.Information("Scoped message");
        }

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Properties["Scope"].ToString().Should().Contain("corr-1");
    }

    /// <summary>
    /// Verifies that log level enablement follows the Serilog minimum level mapping.
    /// </summary>
    [Fact]
    public void IsEnabled_ShouldRespectMappedMinimumLevel()
    {
        var sink = new CollectingSink();
        var serilog = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var logger = new Implementations.Logger<TestCategory>(serilog);

        logger.IsEnabled(LogLevel.Information).Should().BeFalse();
        logger.IsEnabled(LogLevel.Error).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the convenience fatal API maps to the Serilog fatal level.
    /// </summary>
    [Fact]
    public void Fatal_ShouldWriteCriticalEvent()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);

        logger.Fatal("Fatal failure");

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Level.Should().Be(LogEventLevel.Fatal);
    }

    private static Serilog.ILogger CreateLogger(ILogEventSink sink)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
    }

    private sealed class TestCategory;

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
