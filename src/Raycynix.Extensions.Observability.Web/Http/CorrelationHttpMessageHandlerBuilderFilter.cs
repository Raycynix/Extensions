using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Raycynix.Extensions.Observability.Web.Http;

/// <summary>
/// Adds correlation propagation to all <see cref="HttpClient"/> instances created by the factory.
/// </summary>
internal sealed class CorrelationHttpMessageHandlerBuilderFilter(IServiceProvider serviceProvider)
    : IHttpMessageHandlerBuilderFilter
{
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);
            builder.AdditionalHandlers.Add(serviceProvider.GetRequiredService<CorrelationHeaderHandler>());
        };
    }
}
