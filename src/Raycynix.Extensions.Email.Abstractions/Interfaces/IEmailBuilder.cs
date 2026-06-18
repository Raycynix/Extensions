using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Email.Abstractions.Interfaces;

/// <summary>
/// Exposes the shared state used by Raycynix email builder extensions.
/// </summary>
public interface IEmailBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration used for email registrations.
    /// </summary>
    IConfiguration Configuration { get; }
}
