using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Contexts;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;
using Serilog;

namespace Raycynix.Extensions.Serilog.Tests.Pipeline;

public sealed class ConfiguratorPipelineTests
{
    [Fact]
    public void InlineConfigurators_ShouldExecuteInAscendingOrder()
    {
        var executionOrder = new List<string>();
        var builder = TestHostBuilderFactory.Create();

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

            logging.ConfigureLogger(
                (_, _) => executionOrder.Add("second"),
                order: 200);

            logging.ConfigureLogger(
                (_, _) => executionOrder.Add("first"),
                order: 100);
        });

        using var host = builder.Build();

        _ = host.Services.GetRequiredService<ILoggerFactory>();

        executionOrder.Should().Equal("first", "second");
    }

    [Fact]
    public void TypedConfigurator_ShouldResolveDependenciesFromContainer()
    {
        var recorder = new ConfiguratorRecorder();

        var builder = TestHostBuilderFactory.Create();

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.ServiceName = "configurator-service";

            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;

            logging.Services.AddSingleton(recorder);

            logging.AddConfigurator<DependencyAwareConfigurator>();
        });

        using var host = builder.Build();

        _ = host.Services.GetRequiredService<ILoggerFactory>();

        recorder.Calls.Should().ContainSingle();

        recorder.ServiceName.Should().Be("configurator-service");

        recorder.EnvironmentName.Should().Be("Testing");
    }

    private sealed class ConfiguratorRecorder
    {
        public List<string> Calls { get; } = [];

        public string? ServiceName { get; set; }

        public string? EnvironmentName { get; set; }

        public IServiceProvider? ServiceProvider { get; set; }
    }

    private sealed class DependencyAwareConfigurator : IRaycynixSerilogConfigurator
    {
        private readonly ConfiguratorRecorder _recorder;

        public DependencyAwareConfigurator(ConfiguratorRecorder recorder)
        {
            _recorder = recorder;
        }

        public int Order => 50;

        public void Configure(
            LoggerConfiguration loggerConfiguration,
            RaycynixSerilogContext context)
        {
            _recorder.Calls.Add(nameof(DependencyAwareConfigurator));

            _recorder.ServiceName = context.Options.ServiceName;

            _recorder.EnvironmentName = context.Environment.EnvironmentName;

            _recorder.ServiceProvider = context.Services;
        }
    }
}