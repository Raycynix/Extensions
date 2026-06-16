using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Email.Abstractions.Interfaces;

namespace Raycynix.Extensions.Email;

/// <summary>
/// Provides a fluent API for extending the Raycynix email registration.
/// </summary>
public sealed class EmailBuilder(
    IServiceCollection services,
    IConfiguration configuration) : IEmailBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } =
        services ?? throw new ArgumentNullException(nameof(services));

    /// <inheritdoc />
    public IConfiguration Configuration { get; } =
        configuration ?? throw new ArgumentNullException(nameof(configuration));
}
