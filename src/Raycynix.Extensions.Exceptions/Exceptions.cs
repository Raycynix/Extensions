using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;
using Raycynix.Extensions.Exceptions.Options;

namespace Raycynix.Extensions.Exceptions;

public static class Exceptions
{
    public static IServiceCollection AddRaycynixExceptions(this IServiceCollection services,
        Action<ExceptionMapperOptions>? configure = null)
    {
        var options = new ExceptionMapperOptions();
        configure?.Invoke(options);

        services.AddSingleton<IExceptionMapper>(new DefaultExceptionMapper(options.Mappings));
        services.AddSingleton<IExceptionDataMasker, DefaultExceptionDataMasker>();

        return services;
    }
}