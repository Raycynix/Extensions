using Microsoft.Extensions.DependencyInjection;
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
        var options = new ExceptionMapperOptions();
        configure?.Invoke(options);

        services.AddSingleton<IExceptionMapper>(new ExceptionMapper(options.Mappings));
        services.AddSingleton<IExceptionDataMasker, ExceptionDataMasker>();
        services.AddSingleton<ITransientExceptionClassifier, TransientExceptionClassifier>();
        services.AddSingleton<IRetryExecutor, RetryExecutor>();
        services.AddSingleton<IBackgroundTaskRunner, BackgroundTaskRunner>();

        return services;
    }
}
