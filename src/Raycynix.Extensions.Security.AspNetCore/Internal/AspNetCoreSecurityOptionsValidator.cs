using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Security.Options;

namespace Raycynix.Extensions.Security.AspNetCore.Internal;

internal sealed class AspNetCoreSecurityOptionsValidator : IConfigurationValidator<SecurityOptions>
{
    public ConfigurationValidationResult Validate(SecurityOptions options)
    {
        return options.JwtOptions is null || string.IsNullOrWhiteSpace(options.JwtOptions.Authority)
            ? ConfigurationValidationResult.Failure(
                "SecurityOptions.JwtOptions.Authority must be provided for ASP.NET Core JWT validation.")
            : ConfigurationValidationResult.Success();
    }
}
