using System.Text;
using System.Text.RegularExpressions;
using Raycynix.Extensions.Contracts.Attributes;
using Raycynix.Extensions.Contracts.Models;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed partial class MessageContractResolver : IMessageContractResolver
{
    public ContractMetadata Resolve(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        var contractAttribute = messageType.GetCustomAttributes(typeof(MessageContractAttribute), false)
            .OfType<MessageContractAttribute>()
            .SingleOrDefault();
        var introducedAttribute = messageType.GetCustomAttributes(typeof(ContractIntroducedAttribute), false)
            .OfType<ContractIntroducedAttribute>()
            .SingleOrDefault();

        var name = contractAttribute?.Name ?? BuildDefaultName(messageType);
        if (!ContractNameRegex().IsMatch(name))
        {
            throw new InvalidOperationException(
                $"Messaging contract name '{name}' for type '{messageType.FullName}' is invalid. Use lowercase letters, digits, dots, and hyphens.");
        }

        return new ContractMetadata
        {
            Name = name,
            Version = ResolveVersion(contractAttribute?.Version, introducedAttribute?.Version, messageType)
        };
    }

    private static ContractVersion ResolveVersion(string? explicitVersion, string? introducedVersion, Type messageType)
    {
        if (!string.IsNullOrWhiteSpace(explicitVersion))
        {
            return ParseVersion(explicitVersion, messageType);
        }

        if (!string.IsNullOrWhiteSpace(introducedVersion))
        {
            return ParseVersion(introducedVersion, messageType);
        }

        return new ContractVersion();
    }

    private static ContractVersion ParseVersion(string version, Type messageType)
    {
        try
        {
            return ContractVersion.Parse(version);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Messaging contract version '{version}' for type '{messageType.FullName}' is invalid.",
                exception);
        }
    }

    private static string BuildDefaultName(Type messageType)
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(messageType.Namespace))
        {
            segments.AddRange(messageType.Namespace
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ToKebabCase));
        }

        segments.Add(ToKebabCase(messageType.Name));
        return string.Join('.', segments);
    }

    private static string ToKebabCase(string value)
    {
        var sanitized = value.Split('`')[0];
        var builder = new StringBuilder(sanitized.Length * 2);

        for (int index = 0; index < sanitized.Length; index++)
        {
            var character = sanitized[index];
            if (char.IsUpper(character) && index > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    [GeneratedRegex("^[a-z0-9]+(?:[.-][a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ContractNameRegex();
}
