using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Internal;

namespace Raycynix.Extensions.Email;

/// <summary>
/// Provides service registration extensions for the Raycynix email package.
/// </summary>
public static class Email
{
    /// <summary>
    /// Extends service collections with Raycynix email registration APIs.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the shared Raycynix email infrastructure.
        /// </summary>
        /// <param name="configuration">The application configuration used to bind <see cref="EmailConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound email configuration.</param>
        /// <returns>A builder that can be used to register an email provider.</returns>
        public IEmailBuilder AddRaycynixEmail(
            IConfiguration configuration,
            Action<EmailConfiguration>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddRaycynixConfiguration<EmailConfiguration>(
                configuration,
                configurePostBind: setup);
            services.AddRaycynixConfigurationValidator<EmailConfiguration, EmailConfigurationValidator>();
            services.TryAddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IConfigurationAccessor<EmailConfiguration>>().Current);
            services.TryAddSingleton(static serviceProvider =>
                EmailProviderDescriptor.Resolve(serviceProvider));

            return new EmailBuilder(services, configuration);
        }
    }
}
