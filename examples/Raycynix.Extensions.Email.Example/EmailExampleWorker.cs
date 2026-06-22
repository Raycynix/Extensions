using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;

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

        logger.LogInformation("Email example prepared message. ToCount={ToCount}, HasHtml={HasHtml}, AttachmentCount={AttachmentCount}.",
            message.To.Count,
            !string.IsNullOrWhiteSpace(message.Body.Html),
            message.Attachments.Count);

        if (!options.Send)
        {
            logger.LogInformation("Email example finished in dry-run mode. Set EmailExample:Send to true to send.");
            applicationLifetime.StopApplication();
            return;
        }

        var result = await emailSender.SendAsync(message, stoppingToken);

        if (result.Succeeded)
        {
            logger.LogInformation(
                "Email sent. Provider={Provider}, HasMessageId={HasMessageId}.",
                result.Provider,
                !string.IsNullOrWhiteSpace(result.MessageId));
        }
        else
        {
            logger.LogWarning(
                "Email provider rejected the message. Provider={Provider}, ErrorCode={ErrorCode}.",
                result.Provider,
                result.ErrorCode);
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
