using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Configurations;
using Raycynix.Extensions.Messaging.Grpc.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Internal;

namespace Raycynix.Extensions.Messaging.Grpc;

/// <summary>
/// Registers gRPC direct request/response services for Raycynix messaging.
/// </summary>
public static class GrpcMessagingBuilderExtensions
{
    /// <param name="builder">The messaging builder.</param>
    extension(MessagingBuilder builder)
    {
        /// <summary>
        /// Enables the gRPC direct transport for the current messaging builder.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="sectionName">An optional configuration section name. Defaults to <c>GrpcDirectMessagingConfiguration</c>.</param>
        /// <param name="setup">An optional callback for adjusting the bound configuration.</param>
        /// <returns>The same messaging builder instance.</returns>
        public MessagingBuilder AddGrpc(
            IConfiguration configuration,
            string? sectionName = null,
            Action<GrpcDirectMessagingConfiguration>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configuration);

            var options = new GrpcDirectMessagingConfiguration();
            configuration.GetSection(sectionName ?? nameof(GrpcDirectMessagingConfiguration)).Bind(options);
            setup?.Invoke(options);

            return builder.AddGrpc(options);
        }

        /// <summary>
        /// Enables the gRPC direct transport for the current messaging builder.
        /// </summary>
        /// <param name="setup">The gRPC configuration callback.</param>
        /// <returns>The same messaging builder instance.</returns>
        public MessagingBuilder AddGrpc(Action<GrpcDirectMessagingConfiguration> setup)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(setup);

            var configuration = new GrpcDirectMessagingConfiguration();
            setup(configuration);
            return builder.AddGrpc(configuration);
        }

        /// <summary>
        /// Registers a unary gRPC operation that can be invoked through the shared request client abstraction.
        /// </summary>
        /// <typeparam name="TGrpcClient">The generated gRPC client type.</typeparam>
        /// <typeparam name="TRequest">The request payload type.</typeparam>
        /// <typeparam name="TResponse">The response payload type.</typeparam>
        /// <param name="destination">The logical destination key.</param>
        /// <param name="send">The delegate that invokes the unary gRPC call.</param>
        /// <returns>The same messaging builder instance.</returns>
        public MessagingBuilder AddGrpcUnary<TGrpcClient, TRequest, TResponse>(string destination,
            Func<TGrpcClient, TRequest, CancellationToken, Task<TResponse>> send)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrWhiteSpace(destination);
            ArgumentNullException.ThrowIfNull(send);

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IGrpcRequestOperation>(
                    new GrpcRequestOperation<TGrpcClient, TRequest, TResponse>(destination, send)));

            return builder;
        }

        private MessagingBuilder AddGrpc(GrpcDirectMessagingConfiguration configuration)
        {
            configuration.Validate();

            builder.Services.TryAddSingleton(configuration);
            builder.Services.TryAddSingleton<IGrpcClientFactory, GrpcClientFactory>();
            builder.Services.TryAddScoped<IGrpcRequestProcessor, GrpcRequestProcessor>();
            builder.Services.TryAddSingleton<IGrpcRequestClient, GrpcRequestClient>();
            builder.Services.Replace(ServiceDescriptor.Singleton<IDirectRequestClient, GrpcRequestClient>());

            return builder;
        }
    }
}
