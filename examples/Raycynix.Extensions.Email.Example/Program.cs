using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Email;
using Raycynix.Extensions.Email.Example;
using Raycynix.Extensions.Email.Smtp;
using Raycynix.Extensions.Secrets;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, configuration) =>
    {
        configuration.UseRaycynixConfigurationSources(options =>
        {
            options.BasePath = AppContext.BaseDirectory;
            options.BaseFileName = "appsettings";
            options.EnvironmentName = context.HostingEnvironment.EnvironmentName;
            options.IncludeUserSecrets = context.HostingEnvironment.IsDevelopment();
        });
    });

builder
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixSecrets();

        services
            .AddRaycynixEmail(context.Configuration)
            .AddSmtp(smtp =>
            {
                smtp.Username = ResolveSecret(
                    context.Configuration,
                    "EmailConfiguration:SmtpConfiguration:Username");
                smtp.Password = ResolveSecret(
                    context.Configuration,
                    "EmailConfiguration:SmtpConfiguration:Password");
            });

        services.AddHostedService<EmailExampleWorker>();
    });

await builder.RunConsoleAsync();

static string? ResolveSecret(IConfiguration configuration, string key)
{
    var secretServices = new ServiceCollection();
    secretServices.AddSingleton(configuration);
    secretServices.AddRaycynixSecrets();

    using var secretProvider = secretServices.BuildServiceProvider();
    var secrets = secretProvider.GetRequiredService<ISecretResolver>();

    return secrets
        .GetSecretAsync(key)
        .AsTask()
        .GetAwaiter()
        .GetResult();
}
