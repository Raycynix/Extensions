using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Exposes the shared state used by Raycynix database builder extensions.
/// </summary>
public interface IDatabaseBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration used for database registrations.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the assembly that initiated the database registration.
    /// </summary>
    public Assembly CallerAssembly { get; }
}
