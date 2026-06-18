using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Smtp.Configurations;
using Raycynix.Extensions.Email.Smtp.Internal;

namespace Raycynix.Extensions.Email.Smtp;

/// <summary>
/// Provides SMTP-specific email registration extensions.
/// </summary>
public static class Email
{
    /// <summary>
    /// Adds SMTP provider support to the shared Raycynix email registration.
    /// </summary>
    /// <param name="builder">The shared email builder.</param>
    /// <param name="configure">An optional callback for adjusting SMTP-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IEmailBuilder AddSmtp(
        this IEmailBuilder builder,
        Action<SmtpConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<SmtpConfiguration>(
            builder.Configuration,
            $"{nameof(EmailConfiguration)}:{nameof(SmtpConfiguration)}",
            configurePostBind: configure);
        builder.Services.AddRaycynixConfigurationValidator<SmtpConfiguration, SmtpConfigurationValidator>();
        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<SmtpConfiguration>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IEmailProviderRegistration, SmtpEmailProviderRegistration>());
        builder.Services.TryAddSingleton<SmtpMimeMessageFactory>();
        builder.Services.TryAddTransient<IEmailSender, SmtpEmailSender>();

        return builder;
    }
}
