using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Elastic;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;
using Serilog.Core;

namespace Raycynix.Extensions.Serilog.Tests.Pipeline;

public sealed class FallbackConsoleTests
{
    [Fact]
    public void NoNativeSinks_ShouldUseFallbackConsole()
    {
        var originalOutput = Console.Out;

        using var output = new StringWriter();

        Console.SetOut(output);

        try
        {
            var builder = TestHostBuilderFactory.Create();

            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.DefaultConsoleOutputTemplate = "FALLBACK:{Message:lj}{NewLine}";
            });

            using var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

            logger.LogInformation("Fallback console message");

            output.Flush();

            output.ToString().Should().Contain("FALLBACK:Fallback console message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void NativeConsoleSink_ShouldPreventFallbackConsoleDuplication()
    {
        var originalOutput = Console.Out;

        using var output = new StringWriter();

        Console.SetOut(output);

        try
        {
            var builder = TestHostBuilderFactory.Create();

            builder.Configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Serilog:Using:0"] = "Serilog.Sinks.Console",

                    ["Serilog:WriteTo:0:Name"] = "Console",

                    ["Serilog:WriteTo:0:Args:outputTemplate"] = "NATIVE:{Message:lj}{NewLine}"
                });

            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.DefaultConsoleOutputTemplate = "FALLBACK:{Message:lj}{NewLine}";
            });

            using var host = builder.Build();

            var logger = host.Services.GetRequiredService<ILogger<TestCategory>>();

            logger.LogInformation("Native console message");

            output.Flush();

            var renderedOutput = output.ToString();

            renderedOutput.Should().Contain("NATIVE:Native console message");

            renderedOutput.Should().NotContain("FALLBACK:");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void ProgrammaticSink_ShouldPreventFallbackConsole()
    {
        var sink = new CollectingSink();
        var originalOutput = Console.Out;

        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            var builder = TestHostBuilderFactory.Create();

            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.DefaultConsoleOutputTemplate = "FALLBACK:{Message:lj}{NewLine}";
                logging.ConfigureSink((_, configuration) => configuration.WriteTo.Sink(sink));
            });

            using var host = builder.Build();
            host.Services.GetRequiredService<ILogger<TestCategory>>()
                .LogInformation("Programmatic sink message");

            sink.Single("Programmatic sink message");
            output.ToString().Should().NotContain("FALLBACK:");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void DependencyInjectedSink_ShouldPreventFallbackConsole()
    {
        var sink = new CollectingSink();
        var originalOutput = Console.Out;

        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            var builder = TestHostBuilderFactory.Create();
            builder.Services.AddSingleton<ILogEventSink>(sink);

            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.DefaultConsoleOutputTemplate = "FALLBACK:{Message:lj}{NewLine}";
            });

            using var host = builder.Build();
            host.Services.GetRequiredService<ILogger<TestCategory>>()
                .LogInformation("Dependency injected sink message");

            sink.Single("Dependency injected sink message");
            output.ToString().Should().NotContain("FALLBACK:");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void DisabledElasticSink_ShouldStillUseFallbackConsole()
    {
        var originalOutput = Console.Out;

        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            var builder = TestHostBuilderFactory.Create();

            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.DefaultConsoleOutputTemplate = "FALLBACK:{Message:lj}{NewLine}";
                logging.AddElastic(options => options.Enabled = false);
            });

            using var host = builder.Build();
            host.Services.GetRequiredService<ILogger<TestCategory>>()
                .LogInformation("Disabled Elastic message");

            output.ToString().Should().Contain("FALLBACK:Disabled Elastic message");
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private sealed class TestCategory;
}
