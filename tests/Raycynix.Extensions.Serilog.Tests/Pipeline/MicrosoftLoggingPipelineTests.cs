using FluentAssertions;
using global::Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;
using Serilog.Core;

namespace Raycynix.Extensions.Serilog.Tests.Pipeline;

public sealed class MicrosoftLoggingPipelineTests
{
    [Fact]
    public void MicrosoftLogger_ShouldPreserveStructuredMessageTemplate()
    {
        var sink = new CollectingSink();

        using var host = CreateHost(sink);

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogWarning("Price recalculated for {ProductId}", "sku-1");

        var entry = sink.Single("Price recalculated for {ProductId}");

        entry.Level.Should().Be(LogEventLevel.Warning);

        entry.Properties["ProductId"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("sku-1");
    }

    [Fact]
    public void MicrosoftLogger_ShouldPreserveException()
    {
        var sink = new CollectingSink();

        using var host = CreateHost(sink);

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        var exception =
            new InvalidOperationException("boom");

        logger.LogError(exception, "Order {OrderId} failed", "ORD-42");

        var entry = sink.Single("Order {OrderId} failed");

        entry.Exception.Should().BeSameAs(exception);

        entry.Properties["OrderId"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("ORD-42");
    }

    [Fact]
    public void MicrosoftLogger_ShouldFlattenDictionaryScope()
    {
        var sink = new CollectingSink();

        using var host = CreateHost(sink);

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = "corr-123",
                   ["UserId"] = "user-7"
               }))
        {
            logger.LogInformation("Scoped operation");
        }

        var entry = sink.Single("Scoped operation");

        entry.Properties["CorrelationId"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("corr-123");

        entry.Properties["UserId"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("user-7");
    }

    [Fact]
    public void MicrosoftLogger_ShouldPreserveEventId()
    {
        var sink = new CollectingSink();

        using var host = CreateHost(sink);

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation(new EventId(42, "OrderAccepted"), "Order accepted");

        var entry = sink.Single("Order accepted");

        var eventId = entry.Properties["EventId"]
            .Should()
            .BeOfType<StructureValue>()
            .Which;

        var id = eventId.Properties
            .Single(current => current.Name == "Id")
            .Value
            .Should()
            .BeOfType<ScalarValue>()
            .Which;

        var name = eventId.Properties
            .Single(current => current.Name == "Name")
            .Value
            .Should()
            .BeOfType<ScalarValue>()
            .Which;

        id.Value.Should().Be(42);
        name.Value.Should().Be("OrderAccepted");
    }

    [Fact]
    public void Pipeline_ShouldAddRaycynixMetadataAndSourceContext()
    {
        var sink = new CollectingSink();

        using var host = CreateHost(sink, logging =>
        {
            logging.Options.ServiceName = "orders-api";

            logging.Options.ServiceVersion = "2.1.0";

            logging.Options.Environment = "Testing";
        });

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Metadata event");

        var entry = sink.Single("Metadata event");

        entry.Properties["ServiceName"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("orders-api");

        entry.Properties["ServiceVersion"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("2.1.0");

        entry.Properties["Environment"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be("Testing");

        entry.Properties["SourceContext"]
            .Should()
            .BeOfType<ScalarValue>()
            .Which.Value.Should()
            .Be(typeof(TestCategory).FullName!.Replace('+', '.'));
    }

    [Fact]
    public void ReadFromServices_ShouldDiscoverRegisteredSink()
    {
        var sink = new CollectingSink();
        var builder = TestHostBuilderFactory.Create();

        builder.Services.AddSingleton<ILogEventSink>(sink);

        builder.AddRaycynixSerilog(logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; });

        using var host = builder.Build();

        var logger = host.Services
            .GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Dependency injected sink");

        sink.Single("Dependency injected sink")
            .Level.Should()
            .Be(LogEventLevel.Information);
    }

    [Fact]
    public void NativeSerilogConfiguration_ShouldControlMinimumLevel()
    {
        var sink = new CollectingSink();
        var builder = TestHostBuilderFactory.Create();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Raycynix:Serilog:SerilogSectionName"] = "ApplicationSerilog",

                ["ApplicationSerilog:MinimumLevel:Default"] = "Warning"
            });

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

            logging.ConfigureSink((_, loggerConfiguration) =>
                loggerConfiguration.WriteTo.Sink(sink));
        });

        using var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

        logger.LogInformation("Suppressed information");

        logger.LogWarning("Allowed warning");

        sink.Events.Should()
            .NotContain(current => current.MessageTemplate.Text == "Suppressed information");

        sink.Single("Allowed warning")
            .Level.Should()
            .Be(LogEventLevel.Warning);
    }

    private static IHost CreateHost(
        CollectingSink sink,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        var builder = TestHostBuilderFactory.Create();

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

            configure?.Invoke(logging);

            logging.ConfigureSink((_, loggerConfiguration) =>
                loggerConfiguration.WriteTo.Sink(sink));
        });

        return builder.Build();
    }

    private sealed class TestCategory;
}
