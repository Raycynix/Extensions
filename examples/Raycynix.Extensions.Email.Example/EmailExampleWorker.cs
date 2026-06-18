using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Email.Example;

internal sealed class EmailExampleWorker(
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<EmailExampleWorker> logger,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = configuration.GetSection("EmailExample").Get<EmailExampleConfigurations>()
                      ?? new EmailExampleConfigurations();
        var message = CreateMessage(options);

        logger.Information("Email example prepared message\n{Message}", new
        {
            To = message.To.Select(static recipient => recipient.Address).ToArray(),
            message.Subject,
            HasHtml = !string.IsNullOrWhiteSpace(message.Body.Html),
            Attachments = message.Attachments.Select(static attachment => attachment.FileName).ToArray()
        });

        if (!options.Send)
        {
            logger.Information("Email example finished in dry-run mode. Set EmailExample:Send to true to send.");
            applicationLifetime.StopApplication();
            return;
        }

        var result = await emailSender.SendAsync(message, stoppingToken);

        if (result.Succeeded)
        {
            logger.Information("Email sent\n{Result}", new
            {
                result.Provider,
                result.MessageId
            });
        }
        else
        {
            logger.Warning("Email provider rejected the message\n{Result}", new
            {
                result.Provider,
                result.ErrorCode,
                result.ErrorMessage
            });
        }

        applicationLifetime.StopApplication();
    }

    private static EmailMessage CreateMessage(EmailExampleConfigurations options)
    {
        var attachment = EmailAttachment.FromBytes(
            "welcome.txt",
            "Welcome to Raycynix email."u8.ToArray(),
            "text/plain");

        return new EmailMessage
        {
            To = [new EmailAddress(options.ToAddress, options.ToDisplayName)],
            Subject = "Raycynix email example",
            Body = EmailBody.FromHtml(
                "<strong>Hello from Raycynix email.</strong>",
                "Hello from Raycynix email."),
            Headers = new Dictionary<string, string>
            {
                ["X-Raycynix-Example"] = "email"
            },
            Metadata = new Dictionary<string, string>
            {
                ["Example"] = "Email"
            },
            Attachments = [attachment]
        };
    }

    private sealed class EmailExampleConfigurations
    {
        public bool Send { get; set; }

        public string ToAddress { get; set; } = "recipient@example.com";

        public string? ToDisplayName { get; set; } = "Example Recipient";
    }
}
