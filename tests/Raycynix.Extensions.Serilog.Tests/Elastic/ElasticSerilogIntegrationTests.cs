using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Serilog.Elastic;
using Raycynix.Extensions.Serilog.Elastic.Configurations;
using Raycynix.Extensions.Serilog.Elastic.Enums;
using Raycynix.Extensions.Serilog.Elastic.Internal;
using Raycynix.Extensions.Serilog.Tests.Infrastructure;

namespace Raycynix.Extensions.Serilog.Tests.Elastic;

public sealed class ElasticSerilogIntegrationTests
{
    [Fact]
    public void AddElastic_ShouldBindNestedConfiguration()
    {
        var builder = TestHostBuilderFactory.Create();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Raycynix:Serilog:Elastic:Nodes:0"] = "https://localhost:9200",
                ["Raycynix:Serilog:Elastic:Authentication:Mode"] = "ApiKey",
                ["Raycynix:Serilog:Elastic:Authentication:ApiKey"] = "encoded-key",
                ["Raycynix:Serilog:Elastic:DataStream:Dataset"] = "orders",
                ["Raycynix:Serilog:Elastic:Buffer:ExportMaxRetries"] = "5"
            });

        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;
            logging.AddElastic();
        });

        using var host = builder.Build();
        var options = host.Services.GetRequiredService<ElasticSerilogOptions>();

        options.Nodes.Should().ContainSingle().Which.Should().Be(new Uri("https://localhost:9200"));
        options.Authentication.Mode.Should().Be(ElasticAuthenticationMode.ApiKey);
        options.Authentication.ApiKey.Should().Be("encoded-key");
        options.DataStream.Dataset.Should().Be("orders");
        options.Buffer.ExportMaxRetries.Should().Be(5);
    }

    [Fact]
    public void AddElastic_WhenDisabled_ShouldNotRequireConnection()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () => builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;
            logging.AddElastic(options => options.Enabled = false);
        });

        registration.Should().NotThrow();
    }

    [Fact]
    public void AddElastic_WithoutNodes_ShouldFailFast()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () => builder.AddRaycynixSerilog(logging =>
            logging.AddElastic());

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*At least one Elasticsearch node*");
    }

    [Fact]
    public void AddElastic_WhenRegisteredTwice_ShouldThrow()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () => builder.AddRaycynixSerilog(logging =>
        {
            logging.AddElastic(options => options.Enabled = false);
            logging.AddElastic(options => options.Enabled = false);
        });

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already been registered*");
    }

    [Fact]
    public void AddElastic_WithCloudConnectionAndNoAuthentication_ShouldFailFast()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () => builder.AddRaycynixSerilog(logging =>
            logging.AddElastic(options =>
            {
                options.ConnectionMode = ElasticConnectionMode.ElasticCloud;
                options.CloudId = "deployment:encoded";
            }));

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*requires API key or basic authentication*");
    }

    [Fact]
    public void AddElastic_WithInvalidBufferConfiguration_ShouldFailFast()
    {
        var builder = TestHostBuilderFactory.Create();

        var registration = () => builder.AddRaycynixSerilog(logging =>
            logging.AddElastic(options =>
            {
                options.Nodes.Add(new Uri("https://localhost:9200"));
                options.Buffer.ExportMaxConcurrency = 0;
            }));

        registration.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ExportMaxConcurrency*greater than zero*");
    }

    [Fact]
    public void DataStreamName_ShouldUseNormalizedApplicationMetadata()
    {
        var builder = TestHostBuilderFactory.Create();
        builder.AddRaycynixSerilog(logging =>
        {
            logging.Options.ServiceName = "Orders.Api";
            logging.Options.Environment = "QA West";
            logging.Options.UseDefaultConsoleWhenNoSinksConfigured = false;
        });

        using var host = builder.Build();
        var context = new global::Raycynix.Extensions.Serilog.Contexts.RaycynixSerilogContext(
            builder.Configuration,
            builder.Environment,
            host.Services,
            host.Services.GetRequiredService<global::Raycynix.Extensions.Serilog.Configurations.RaycynixSerilogOptions>());

        var name = ElasticDataStreamNameResolver.Resolve(new ElasticDataStreamOptions(), context);

        name.ToString().Should().Be("logs-orders_api-qa_west");
    }
}
