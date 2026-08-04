using Raycynix.Extensions.Serilog.Elastic.Configurations;
using Raycynix.Extensions.Serilog.Elastic.Enums;

namespace Raycynix.Extensions.Serilog.Elastic.Internal;

internal static class ElasticSerilogOptionsValidator
{
    public static void Validate(
        ElasticSerilogOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
            return;

        ValidateConnection(options);
        ValidateAuthentication(options);
        ValidateDataStream(options.DataStream);
        ValidateProxy(options.Proxy);
        ValidateBuffer(options.Buffer);
    }

    private static void ValidateConnection(
        ElasticSerilogOptions options)
    {
        switch (options.ConnectionMode)
        {
            case ElasticConnectionMode.Elasticsearch:
                ValidateNodes(options.Nodes);
                break;

            case ElasticConnectionMode.ElasticCloud:
                if (string.IsNullOrWhiteSpace(options.CloudId))
                    throw new InvalidOperationException(
                        "Elastic Cloud ID must be provided when ConnectionMode is ElasticCloud.");

                if (options.UseSniffing)
                    throw new InvalidOperationException("Node sniffing cannot be enabled in Elastic Cloud mode.");
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(options.ConnectionMode),
                    options.ConnectionMode,
                    "Unsupported Elastic connection mode.");
        }
    }

    private static void ValidateNodes(IEnumerable<Uri> nodes)
    {
        var nodeArray = nodes.ToArray();

        if (nodeArray.Length == 0)
            throw new InvalidOperationException("At least one Elasticsearch node must be configured.");

        foreach (var node in nodeArray)
        {
            if (!node.IsAbsoluteUri)
                throw new InvalidOperationException($"Elasticsearch node '{node}' must be an absolute URI.");

            if (node.Scheme is not ("http" or "https"))
                throw new InvalidOperationException($"Elasticsearch node '{node}' must use HTTP or HTTPS.");
        }
    }

    private static void ValidateAuthentication(ElasticSerilogOptions options)
    {
        var authentication = options.Authentication;

        switch (authentication.Mode)
        {
            case ElasticAuthenticationMode.None:
                if (options.ConnectionMode ==
                    ElasticConnectionMode.ElasticCloud)
                    throw new InvalidOperationException("Elastic Cloud requires API key or basic authentication.");
                break;

            case ElasticAuthenticationMode.ApiKey:
                if (string.IsNullOrWhiteSpace(authentication.ApiKey))
                    throw new InvalidOperationException(
                        "Elastic API key cannot be empty.");
                break;

            case ElasticAuthenticationMode.Basic:
                if (string.IsNullOrWhiteSpace(authentication.Username))
                    throw new InvalidOperationException("Elastic username cannot be empty.");

                if (string.IsNullOrWhiteSpace(authentication.Password))
                    throw new InvalidOperationException("Elastic password cannot be empty.");
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(authentication.Mode),
                    authentication.Mode,
                    "Unsupported Elastic authentication mode.");
        }
    }

    private static void ValidateDataStream(
        ElasticDataStreamOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Type))
            throw new InvalidOperationException("Elastic data stream type cannot be empty.");

        if (options.Dataset is not null &&
            string.IsNullOrWhiteSpace(options.Dataset))
            throw new InvalidOperationException("Elastic data stream dataset cannot contain only whitespace.");

        if (options.Namespace is not null &&
            string.IsNullOrWhiteSpace(options.Namespace))
            throw new InvalidOperationException("Elastic data stream namespace cannot contain only whitespace.");
    }

    private static void ValidateProxy(
        ElasticProxyOptions options)
    {
        if (options.Url is null)
        {
            if (!string.IsNullOrWhiteSpace(options.Username) ||
                !string.IsNullOrWhiteSpace(options.Password))
                throw new InvalidOperationException("A proxy URL must be configured when proxy credentials are used.");

            return;
        }

        if (!options.Url.IsAbsoluteUri)
            throw new InvalidOperationException("Elastic proxy URL must be absolute.");

        var hasUsername =
            !string.IsNullOrWhiteSpace(options.Username);

        var hasPassword =
            !string.IsNullOrWhiteSpace(options.Password);

        if (hasUsername != hasPassword)
            throw new InvalidOperationException("Both proxy username and proxy password must be configured.");
    }

    private static void ValidateBuffer(
        ElasticBufferOptions options)
    {
        ValidateNonNegative(
            options.ExportMaxRetries,
            nameof(options.ExportMaxRetries));

        ValidatePositive(
            options.ExportMaxConcurrency,
            nameof(options.ExportMaxConcurrency));

        ValidatePositive(
            options.InboundBufferMaxSize,
            nameof(options.InboundBufferMaxSize));

        ValidatePositive(
            options.OutboundBufferMaxSize,
            nameof(options.OutboundBufferMaxSize));

        if (options.OutboundBufferMaxLifetime <= TimeSpan.Zero)
            throw new InvalidOperationException(
                $"{nameof(options.OutboundBufferMaxLifetime)} must be greater than zero.");
    }

    private static void ValidatePositive(int? value, string propertyName)
    {
        if (value is <= 0)
            throw new InvalidOperationException($"{propertyName} must be greater than zero.");
    }

    private static void ValidateNonNegative(int? value, string propertyName)
    {
        if (value is < 0)
            throw new InvalidOperationException($"{propertyName} cannot be negative.");
    }
}