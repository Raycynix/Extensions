using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Email;
using Raycynix.Extensions.Email.Example;
using Raycynix.Extensions.Email.Smtp;
using Raycynix.Extensions.Logging;
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
    .UseRaycynixLogging()
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixLogging(context.Configuration);
        services.AddRaycynixSecrets();

        services
            .AddRaycynixEmail(context.Configuration)
            .AddSmtp(smtp =>
            {
                using var secretProvider = services.BuildServiceProvider();
                var secrets = secretProvider.GetRequiredService<ISecretResolver>();

                smtp.Username = secrets
                    .GetSecretAsync("EmailConfiguration:SmtpConfiguration:Username")
                    .AsTask()
                    .GetAwaiter()
                    .GetResult();
                smtp.Password = secrets
                    .GetSecretAsync("EmailConfiguration:SmtpConfiguration:Password")
                    .AsTask()
                    .GetAwaiter()
                    .GetResult();
            });

        services.AddHostedService<EmailExampleWorker>();
    });

await builder.RunConsoleAsync();
