using FluentAssertions;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
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
    /// Verifies that log entries preserve the mapped severity and structured template arguments.
    /// </summary>
    [Fact]
    public void Log_ShouldWriteStructuredEvent_WithMappedLevel()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);

        logger.Log(LogLevel.Warning, null, "Price recalculated for {ProductId}", "sku-1");

        sink.Events.Should().ContainSingle();
        var entry = sink.Events.Single();
        entry.Level.Should().Be(LogEventLevel.Warning);
        entry.MessageTemplate.Text.Should().Be("Price recalculated for {ProductId}");
        entry.Properties["ProductId"].ToString().Should().Be("\"sku-1\"");
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

        logger.Error(exception, "Failure during {Operation}", "checkout");

        sink.Events.Should().ContainSingle();
        var entry = sink.Events.Single();
        entry.Exception.Should().BeSameAs(exception);
        entry.MessageTemplate.Text.Should().Be("Failure during {Operation}");
        entry.Properties["Operation"].ToString().Should().Be("\"checkout\"");
    }

    /// <summary>
    /// Verifies that correlation id is taken from the ambient operation context when available.
    /// </summary>
    [Fact]
    public void Log_ShouldAttachCorrelationId_FromOperationContext()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);
        var previous = OperationContext.Current;
        OperationContext.Current = new OperationContext
        {
            CorrelationId = "corr-123"
        };

        try
        {
            logger.Information("Correlated message");
        }
        finally
        {
            OperationContext.Current = previous;
        }

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Properties["CorrelationId"].ToString().Should().Be("\"corr-123\"");
    }

    /// <summary>
    /// Verifies that plain messages without template arguments do not create unrelated structured properties.
    /// </summary>
    [Fact]
    public void Log_ShouldNotWriteStructuredProperties_WhenTemplateHasNoArguments()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        var logger = new Implementations.Logger<TestCategory>(serilog);

        logger.Information("Message without metadata");

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Properties.Should().NotContainKey("Metadata");
        sink.Events.Single().Properties.Should().NotContainKey("ProductId");
    }

    /// <summary>
    /// Verifies that the Microsoft logger interface path emits the formatter result as the final message.
    /// </summary>
    [Fact]
    public void MicrosoftLoggerLog_ShouldUseFormatterOutput()
    {
        var sink = new CollectingSink();
        var serilog = CreateLogger(sink);
        ILogger<TestCategory> logger =
            new Implementations.Logger<TestCategory>(serilog);
        var state = new Dictionary<string, object?> { ["ProductId"] = "sku-1" };

        logger.Log(LogLevel.Information, new EventId(42, "PriceRead"), state, null,
            static (current, _) => $"Read {current["ProductId"]}");

        sink.Events.Should().ContainSingle();
        var entry = sink.Events.Single();
        entry.RenderMessage().Should().Be("Read sku-1");
        entry.Exception.Should().BeNull();
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
