using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;

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

    private sealed class TestCategory;
}