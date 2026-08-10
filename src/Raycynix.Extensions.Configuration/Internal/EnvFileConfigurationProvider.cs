using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class EnvFileConfigurationSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);

        PhysicalFileProvider? ownedFileProvider = null;
        if (FileProvider is PhysicalFileProvider physicalFileProvider)
        {
            ownedFileProvider = new PhysicalFileProvider(
                physicalFileProvider.Root,
                ExclusionFilters.None);
            FileProvider = ownedFileProvider;
        }

        return new EnvFileConfigurationProvider(this, ownedFileProvider);
    }
}

internal sealed class EnvFileConfigurationProvider(
    EnvFileConfigurationSource source,
    PhysicalFileProvider? ownedFileProvider)
    : FileConfigurationProvider(source), IDisposable
{
    public override void Load(Stream stream)
    {
        Data = EnvFileParser.Parse(stream);
    }

    void IDisposable.Dispose()
    {
        base.Dispose();
        ownedFileProvider?.Dispose();
    }
}

internal static class EnvFileParser
{
    public static IDictionary<string, string?> Parse(Stream stream)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            ParseLine(line, lineNumber, values);
        }

        return values;
    }

    private static void ParseLine(
        string line,
        int lineNumber,
        IDictionary<string, string?> values)
    {
        var content = line.Trim();
        if (content.Length == 0 || content[0] == '#')
        {
            return;
        }

        if (content.StartsWith("export", StringComparison.Ordinal) &&
            content.Length > "export".Length &&
            char.IsWhiteSpace(content["export".Length]))
        {
            content = content["export".Length..].TrimStart();
        }

        var separatorIndex = content.IndexOf('=');
        if (separatorIndex <= 0)
        {
            throw new FormatException($"Invalid .env entry at line {lineNumber}: expected KEY=VALUE.");
        }

        var key = content[..separatorIndex].Trim();
        if (key.Length == 0 || key.Any(char.IsWhiteSpace))
        {
            throw new FormatException($"Invalid .env key at line {lineNumber}.");
        }

        var value = ParseValue(content[(separatorIndex + 1)..], lineNumber);
        values[NormalizeKey(key)] = value;
    }

    private static string ParseValue(string value, int lineNumber)
    {
        var trimmed = value.TrimStart();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return trimmed[0] switch
        {
            '\'' => ParseQuotedValue(trimmed, '\'', lineNumber, unescape: false),
            '"' => ParseQuotedValue(trimmed, '"', lineNumber, unescape: true),
            _ => ParseUnquotedValue(trimmed)
        };
    }

    private static string ParseQuotedValue(string value, char quote, int lineNumber, bool unescape)
    {
        var closingQuoteIndex = FindClosingQuote(value, quote);
        if (closingQuoteIndex < 0)
        {
            throw new FormatException($"Unterminated quoted .env value at line {lineNumber}.");
        }

        var trailing = value[(closingQuoteIndex + 1)..].Trim();
        if (trailing.Length > 0 && trailing[0] != '#')
        {
            throw new FormatException($"Unexpected content after quoted .env value at line {lineNumber}.");
        }

        var content = value[1..closingQuoteIndex];
        return unescape ? UnescapeDoubleQuotedValue(content) : content;
    }

    private static int FindClosingQuote(string value, char quote)
    {
        var escaped = false;
        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (quote == '"' && character == '\\' && !escaped)
            {
                escaped = true;
                continue;
            }

            if (character == quote && !escaped)
            {
                return index;
            }

            escaped = false;
        }

        return -1;
    }

    private static string UnescapeDoubleQuotedValue(string value)
    {
        var result = new StringBuilder(value.Length);

        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '\\' || index == value.Length - 1)
            {
                result.Append(value[index]);
                continue;
            }

            index++;
            switch (value[index])
            {
                case 'n':
                    result.Append('\n');
                    break;
                case 'r':
                    result.Append('\r');
                    break;
                case 't':
                    result.Append('\t');
                    break;
                case '\\':
                    result.Append('\\');
                    break;
                case '"':
                    result.Append('"');
                    break;
                default:
                    result.Append('\\');
                    result.Append(value[index]);
                    break;
            }
        }

        return result.ToString();
    }

    private static string ParseUnquotedValue(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '#' && (index == 0 || char.IsWhiteSpace(value[index - 1])))
            {
                return value[..index].TrimEnd();
            }
        }

        return value.TrimEnd();
    }

    private static string NormalizeKey(string key)
    {
        return key.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
    }
}
