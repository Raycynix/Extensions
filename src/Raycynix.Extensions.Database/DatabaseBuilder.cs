using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides a fluent API for extending the Raycynix database registration.
/// </summary>
public class DatabaseBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    Assembly registrationAssembly) : IDatabaseBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <inheritdoc />
    public IConfiguration Configuration { get; } =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <inheritdoc />
    public Assembly CallerAssembly { get; } =
        registrationAssembly ?? throw new ArgumentNullException(nameof(registrationAssembly));
}
