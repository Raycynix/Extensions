using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Serilog.Configurations;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;

namespace Raycynix.Extensions.Serilog.Tests.Configuration;

public sealed class RaycynixSerilogOptionsTests
{
    [Fact]
    public void AddRaycynixSerilog_ShouldBindRaycynixConfiguration()
    {
        var builder = TestHostBuilderFactory.Create();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Raycynix:Serilog:ServiceName"] = "orders-api",

                ["Raycynix:Serilog:ServiceVersion"] = "1.2.3",

                ["Raycynix:Serilog:Environment"] = "Integration",

                ["Raycynix:Serilog:ApplyDefaultLevelOverrides"] = "false",

                ["Raycynix:Serilog:UseDefaultConsoleWhenNoSinksConfigured"] = "false"
            });

        builder.AddRaycynixSerilog();

        using var host = builder.Build();

        var options = host.Services.GetRequiredService<RaycynixSerilogOptions>();

        options.ServiceName.Should().Be("orders-api");

        options.ServiceVersion.Should().Be("1.2.3");

        options.Environment.Should().Be("Integration");

        options.ApplyDefaultLevelOverrides.Should().BeFalse();

        options.UseDefaultConsoleWhenNoSinksConfigured.Should().BeFalse();
    }

    [Fact]
    public void AddRaycynixSerilog_ShouldResolveMissingHostMetadata()
    {
        var builder = TestHostBuilderFactory.Create();

        var expectedApplicationName = builder.Environment.ApplicationName;

        var expectedEnvironment = builder.Environment.EnvironmentName;

        builder.AddRaycynixSerilog(logging => { logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false; });

        using var host = builder.Build();

        var options = host.Services.GetRequiredService<RaycynixSerilogOptions>();

        options.ServiceName.Should().Be(expectedApplicationName);

        options.Environment.Should().Be(expectedEnvironment);

        options.ServiceVersion.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AddRaycynixSerilog_WithInvalidConsoleTemplate_ShouldThrow()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () =>
            builder.AddRaycynixSerilog(logging =>
            {
                logging.Options.UseDefaultConsoleWhenNoSinksConfigured = true;

                logging.Options.DefaultConsoleOutputTemplate = " ";
            });

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*output template*");
    }
}