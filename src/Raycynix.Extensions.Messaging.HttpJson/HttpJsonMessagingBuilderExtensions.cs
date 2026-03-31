using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Configurations;
using Raycynix.Extensions.Messaging.HttpJson.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Internal;

namespace Raycynix.Extensions.Messaging.HttpJson;

/// <summary>
/// Registers HTTP JSON direct request/response services for Raycynix messaging.
/// </summary>
public static class HttpJsonMessagingBuilderExtensions
{
    internal const string HttpClientName = "Raycynix.Extensions.Messaging.HttpJson";

    /// <summary>
    /// Enables the HTTP JSON direct transport for the current messaging builder.
    /// </summary>
    /// <param name="builder">The messaging builder.</param>
    /// <param name="setup">The HTTP JSON configuration callback.</param>
    /// <returns>The same messaging builder instance.</returns>
    public static MessagingBuilder AddHttpJson(
        this MessagingBuilder builder,
        Action<HttpJsonMessagingConfiguration> setup)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setup);

        var configuration = new HttpJsonMessagingConfiguration();
        setup(configuration);
        configuration.Validate();

        builder.Services.TryAddSingleton(configuration);
        builder.Services.AddHttpClient(HttpClientName, client =>
        {
            client.BaseAddress = new Uri(configuration.BaseAddress, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
        });
        builder.Services.TryAddSingleton<IHttpJsonTransport, HttpJsonTransport>();
        builder.Services.TryAddSingleton<IHttpJsonRequestClient, HttpJsonRequestClient>();
        builder.Services.Replace(ServiceDescriptor.Singleton<IDirectRequestClient, HttpJsonRequestClient>());

        return builder;
    }
}
