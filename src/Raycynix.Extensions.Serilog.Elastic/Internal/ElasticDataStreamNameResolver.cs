using System.Text;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Raycynix.Extensions.Serilog.Contexts;
using Raycynix.Extensions.Serilog.Elastic.Configurations;

namespace Raycynix.Extensions.Serilog.Elastic.Internal;

internal static class ElasticDataStreamNameResolver
{
    public static DataStreamName Resolve(
        ElasticDataStreamOptions options,
        RaycynixSerilogContext context)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);

        var type = NormalizeExplicit(options.Type);

        var dataset = string.IsNullOrWhiteSpace(options.Dataset)
            ? NormalizeGenerated(context.Options.ServiceName)
            : NormalizeExplicit(options.Dataset);

        var dataStreamNamespace =
            string.IsNullOrWhiteSpace(options.Namespace)
                ? NormalizeGenerated(context.Options.Environment)
                : NormalizeExplicit(options.Namespace);

        return new DataStreamName(
            type,
            dataset,
            dataStreamNamespace);
    }

    private static string NormalizeExplicit(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeGenerated(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastCharacterWasSeparator = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastCharacterWasSeparator = false;
                continue;
            }

            if (lastCharacterWasSeparator)
            {
                continue;
            }

            builder.Append('_');
            lastCharacterWasSeparator = true;
        }

        var result = builder
            .ToString()
            .Trim('_');

        return string.IsNullOrWhiteSpace(result)
            ? "application"
            : result;
    }
}