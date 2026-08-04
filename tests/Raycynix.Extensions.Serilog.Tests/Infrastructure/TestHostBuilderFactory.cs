using Microsoft.Extensions.Hosting;

namespace Raycynix.Extensions.Serilog.Tests.Infrastructure;

internal static class TestHostBuilderFactory
{
    public static HostApplicationBuilder Create()
    {
        return new HostApplicationBuilder(
            new HostApplicationBuilderSettings
            {
                ApplicationName = typeof(TestHostBuilderFactory)
                    .Assembly
                    .GetName()
                    .Name,

                EnvironmentName = "Testing",

                ContentRootPath = AppContext.BaseDirectory
            });
    }
}