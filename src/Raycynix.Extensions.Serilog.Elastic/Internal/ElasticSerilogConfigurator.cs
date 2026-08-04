using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Contexts;
using Raycynix.Extensions.Serilog.Elastic.Configurations;
using Raycynix.Extensions.Serilog.Elastic.Enums;
using Serilog;
using Serilog.Core;

namespace Raycynix.Extensions.Serilog.Elastic.Internal;

internal sealed class ElasticSerilogConfigurator(ElasticSerilogOptions options) : IRaycynixSerilogConfigurator
{
    private readonly ElasticSerilogOptions _options =
        options ?? throw new ArgumentNullException(nameof(options));

    public int Order => _options.Order;

    public void Configure(
        LoggerConfiguration loggerConfiguration,
        RaycynixSerilogContext context)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(context);

        if (!_options.Enabled)
        {
            return;
        }

        var dataStream =
            ElasticDataStreamNameResolver.Resolve(
                _options.DataStream,
                context);

        switch (_options.ConnectionMode)
        {
            case ElasticConnectionMode.Elasticsearch:
                ConfigureElasticsearch(loggerConfiguration, dataStream);
                break;

            case ElasticConnectionMode.ElasticCloud:
                ConfigureElasticCloud(loggerConfiguration, dataStream);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(_options.ConnectionMode),
                    _options.ConnectionMode,
                    "Unsupported Elastic connection mode.");
        }
    }

    private void ConfigureElasticsearch(LoggerConfiguration loggerConfiguration, DataStreamName dataStream)
    {
        loggerConfiguration.WriteTo.Elasticsearch(
            nodes: _options.Nodes,
            configureOptions: sinkOptions => ConfigureSinkOptions(sinkOptions, dataStream),
            configureTransport: ConfigureTransport,
            useSniffing: _options.UseSniffing,
            restrictedToMinimumLevel: _options.MinimumLevel);
    }

    private void ConfigureElasticCloud(LoggerConfiguration loggerConfiguration, DataStreamName dataStream)
    {
        var cloudId = _options.CloudId!;

        switch (_options.Authentication.Mode)
        {
            case ElasticAuthenticationMode.ApiKey:
                loggerConfiguration.WriteTo.ElasticCloud(
                    cloudId,
                    _options.Authentication.ApiKey!,
                    configureOptions: sinkOptions => ConfigureSinkOptions(sinkOptions, dataStream),
                    configureTransport: ConfigureTransport,
                    restrictedToMinimumLevel: _options.MinimumLevel);
                break;

            case ElasticAuthenticationMode.Basic: 
                ElasticsearchSinkExtensions.ElasticCloud(
                    loggerConfiguration.WriteTo,
                    cloudId,
                    _options.Authentication.Username!,
                    _options.Authentication.Password!,
                    configureOptions: sinkOptions => ConfigureSinkOptions(sinkOptions, dataStream),
                    configureTransport: ConfigureTransport,
                    restrictedToMinimumLevel: _options.MinimumLevel);
                break;

            default:
                throw new InvalidOperationException("Elastic Cloud requires API key or basic authentication.");
        }
    }

    private void ConfigureSinkOptions(ElasticsearchSinkOptions sinkOptions, DataStreamName dataStream)
    {
        sinkOptions.DataStream = dataStream;
        sinkOptions.BootstrapMethod = _options.BootstrapMethod;

        sinkOptions.IlmPolicy = string.IsNullOrWhiteSpace(_options.DataStream.IlmPolicy)
            ? null
            : _options.DataStream.IlmPolicy.Trim();

        sinkOptions.TextFormatting.IncludeHost = _options.IncludeHost;

        sinkOptions.TextFormatting.IncludeProcess = _options.IncludeProcess;

        sinkOptions.TextFormatting.IncludeUser = _options.IncludeUser;

        sinkOptions.TextFormatting.IncludeActivityData = _options.IncludeActivity;

        if (_options.FilterProperties.Count > 0)
        {
            sinkOptions.TextFormatting.LogEventPropertiesToFilter =
                new HashSet<string>(_options.FilterProperties, StringComparer.Ordinal);
        }

        ConfigureBuffer(sinkOptions);

        _options.ConfigureSinkOptions?.Invoke(sinkOptions);
    }

    private void ConfigureBuffer(ElasticsearchSinkOptions sinkOptions)
    {
        var buffer = _options.Buffer;

        var hasConfiguration =
            buffer.ExportMaxRetries.HasValue ||
            buffer.ExportMaxConcurrency.HasValue ||
            buffer.InboundBufferMaxSize.HasValue ||
            buffer.OutboundBufferMaxSize.HasValue ||
            buffer.OutboundBufferMaxLifetime.HasValue ||
            buffer.FullMode.HasValue;

        if (!hasConfiguration)
            return;

        sinkOptions.ConfigureChannel = channelOptions =>
        {
            var bufferOptions = channelOptions.BufferOptions;

            if (buffer.ExportMaxRetries.HasValue)
                bufferOptions.ExportMaxRetries = buffer.ExportMaxRetries.Value;

            if (buffer.ExportMaxConcurrency.HasValue)
                bufferOptions.ExportMaxConcurrency = buffer.ExportMaxConcurrency.Value;

            if (buffer.InboundBufferMaxSize.HasValue)
                bufferOptions.InboundBufferMaxSize = buffer.InboundBufferMaxSize.Value;

            if (buffer.OutboundBufferMaxSize.HasValue)
                bufferOptions.OutboundBufferMaxSize = buffer.OutboundBufferMaxSize.Value;

            if (buffer.OutboundBufferMaxLifetime.HasValue)
                bufferOptions.OutboundBufferMaxLifetime = buffer.OutboundBufferMaxLifetime.Value;

            if (buffer.FullMode.HasValue)
                bufferOptions.BoundedChannelFullMode = buffer.FullMode.Value;
        };
    }

    private void ConfigureTransport(TransportConfigurationDescriptor transport)
    {
        ApplyAuthentication(transport);
        ApplyProxy(transport);

        if (!string.IsNullOrWhiteSpace(_options.CertificateFingerprint))
            transport.CertificateFingerprint(_options.CertificateFingerprint.Trim());

        if (_options.DebugMode)
            transport.EnableDebugMode();

        _options.ConfigureTransport?.Invoke(transport);
    }

    private void ApplyAuthentication(
        TransportConfigurationDescriptor transport)
    {
        var authentication = _options.Authentication;

        switch (authentication.Mode)
        {
            case ElasticAuthenticationMode.None:
                return;

            case ElasticAuthenticationMode.ApiKey:
                transport.Authentication(new ApiKey(authentication.ApiKey!));
                return;

            case ElasticAuthenticationMode.Basic:
                transport.Authentication(new BasicAuthentication(authentication.Username!, authentication.Password!));
                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(authentication.Mode),
                    authentication.Mode,
                    "Unsupported Elastic authentication mode.");
        }
    }

    private void ApplyProxy(TransportConfigurationDescriptor transport)
    {
        var proxy = _options.Proxy;

        if (proxy.Url is null)
            return;

        if (!string.IsNullOrWhiteSpace(proxy.Username))
        {
            transport.Proxy(
                proxy.Url,
                proxy.Username,
                proxy.Password!);

            return;
        }

        transport.Proxy(proxy.Url);
    }
}