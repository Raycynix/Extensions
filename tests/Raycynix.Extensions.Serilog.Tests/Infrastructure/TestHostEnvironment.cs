using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Raycynix.Extensions.Serilog.Tests.Infrastructure;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Testing";

    public string ApplicationName { get; set; } = "raycynix-serilog-tests";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}