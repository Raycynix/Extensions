using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Example;

internal sealed class ConfigurationExampleWorker(
    IConfigurationAccessor<MessagingOptions> messagingOptionsAccessor,
    IFeatureFlagAccessor featureFlags,
    IApplicationEnvironment applicationEnvironment,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = messagingOptionsAccessor.Current;

        Console.WriteLine("Raycynix configuration example");
        Console.WriteLine($"Environment: {applicationEnvironment.Name}");
        Console.WriteLine($"Consumer: {options.ConsumerName}");
        Console.WriteLine($"ConnectionString: {options.ConnectionString}");
        Console.WriteLine($"BatchSize: {options.BatchSize}");
        Console.WriteLine($"PrefetchCount: {options.PrefetchCount}");
        Console.WriteLine($"UseInboxProcessing: {options.UseInboxProcessing}");
        Console.WriteLine("Feature flags:");

        foreach (var pair in featureFlags.GetAll().OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Console.WriteLine($"  {pair.Key} = {pair.Value}");
        }

        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}