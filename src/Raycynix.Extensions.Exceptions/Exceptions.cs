using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;
using Raycynix.Extensions.Exceptions.Middleware;
using Raycynix.Extensions.Exceptions.Options;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Provides extension methods to add and configure Raycynix exception handling services.
/// The <c>Exceptions</c> class offers functionality to enable exception mapping and data masking
/// by registering related components into the dependency injection container.
/// </summary>
public static class Exceptions
{
    /// <summary>
    /// Adds Raycynix exception handling services to the dependency injection container, allowing for
    /// configurable exception mapping and optional data masking.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the Raycynix exception handling services
    /// will be added.
    /// </param>
    /// <param name="configure">
    /// An optional configuration action to customize the mapping of exceptions to
    /// <see cref="RaycynixException"/> instances via <see cref="ExceptionMapperOptions"/>.
    /// If not specified, the default configuration is used.
    /// </param>
    /// <returns>
    /// The configured <see cref="IServiceCollection"/> to allow for method chaining.
    /// </returns>
    public static IServiceCollection AddRaycynixExceptions(this IServiceCollection services,
        Action<ExceptionMapperOptions>? configure = null)
    {
        var options = new ExceptionMapperOptions();
        configure?.Invoke(options);

        services.AddSingleton<IExceptionMapper>(new DefaultExceptionMapper(options.Mappings));
        services.AddSingleton<IExceptionDataMasker, DefaultExceptionDataMasker>();

        return services;
    }    
    
    /// <summary>
    /// Adds the Raycynix exception handling middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixExceptions(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RaycynixExceptionMiddleware>();
    }
}