using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;
using Raycynix.Extensions.Exceptions.Options;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Provides service registration extensions for the exception handling package.
/// </summary>
public static class Exceptions
{
    /// <summary>
    /// Registers exception mapping, masking, retry, and background execution services.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">An optional callback for custom exception mappings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixExceptions(this IServiceCollection services,
        Action<ExceptionMapperOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ExceptionMapperOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<IExceptionMapper>(new ExceptionMapper(options.Mappings));
        services.TryAddSingleton<IExceptionDataMasker, ExceptionDataMasker>();
        services.TryAddSingleton<ITransientExceptionClassifier, TransientExceptionClassifier>();
        services.TryAddSingleton<IRetryExecutor, RetryExecutor>();
        services.TryAddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();

        return services;
    }
}
