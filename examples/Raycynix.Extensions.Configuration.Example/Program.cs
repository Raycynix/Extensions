using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Configuration.Example;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.UseRaycynixConfigurationSources(options =>
{
    options.EnvironmentName = builder.Environment.EnvironmentName;
    options.IncludeUserSecrets = false;
    options.CommandLineArguments = args;
});

builder.Services.AddRaycynixEnvironment();
builder.Services.AddRaycynixFeatureFlags(builder.Configuration, requireSection: true);
builder.Services.ConfigureRaycynixConfigurationDiagnostics(options =>
{
    options.EnableSnapshots = true;
});
builder.Services.AddRaycynixConfigurationRedactor((key, value) =>
{
    if (key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase))
    {
        return "***";
    }

    return value;
});

builder.Services.AddRaycynixConfiguration<MessagingOptions>(
    builder.Configuration,
    requireSection: true,
    configureDefaults: options =>
    {
        options.ConsumerName = "default-consumer";
        options.BatchSize = 25;
    });
builder.Services.AddRaycynixConfigurationValidator<MessagingOptions, MessagingOptionsValidator>();
builder.Services.AddRaycynixConfigurationReloadPolicy<MessagingOptions>(context =>
{
    if (context.Previous.ConnectionString != context.Current.ConnectionString)
    {
        return ConfigurationReloadResult.Reject(
            "MessagingOptions.ConnectionString cannot be changed at runtime.");
    }

    return ConfigurationReloadResult.Apply();
});
builder.Services.AddRaycynixConfigurationChangeHandler<MessagingOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine(
            $"MessagingOptions changed at {context.ChangedAtUtc:O}. BatchSize={context.Current.BatchSize}, UseInboxProcessing={context.Current.UseInboxProcessing}");

        return ValueTask.CompletedTask;
    });

builder.Services.AddHostedService<ConfigurationExampleWorker>();

await builder.Build().RunAsync();
