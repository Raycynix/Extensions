using Aspire.Hosting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Serilog.Aspire;
using Raycynix.Extensions.Serilog.Configurations;

namespace Raycynix.Extensions.Serilog.Tests.Registration;

public sealed class AspireSerilogRegistrationTests
{
    [Fact]
    public void AddRaycynixSerilog_ShouldRegisterAppHostLogging()
    {
        var builder = DistributedApplication.CreateBuilder([]);

        var result = builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.ServiceName = "test-apphost";
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;
        });

        result.Should().BeSameAs(builder);

        using var services = builder.Services.BuildServiceProvider();

        services.GetRequiredService<RaycynixSerilogOptions>()
            .ServiceName.Should()
            .Be("test-apphost");
    }
}
