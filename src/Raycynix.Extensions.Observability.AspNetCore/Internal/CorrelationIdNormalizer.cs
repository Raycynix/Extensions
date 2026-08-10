using Microsoft.Extensions.Primitives;

namespace Raycynix.Extensions.Observability.AspNetCore.Internal;

internal static class CorrelationIdNormalizer
{
    public const int DefaultMaximumLength = 128;
    public const int MaximumAllowedLength = 1024;

    public static bool TryNormalize(StringValues values, int maximumLength, out string correlationId)
    {
        if (values.Count == 1)
        {
            return TryNormalize(values[0], maximumLength, out correlationId);
        }

        correlationId = string.Empty;
        return false;
    }

    public static bool TryNormalize(string? value, int maximumLength, out string correlationId)
    {
        var candidate = value?.Trim();
        if (!string.IsNullOrEmpty(candidate) &&
            candidate.Length <= maximumLength &&
            candidate.All(IsAllowedCharacter))
        {
            correlationId = candidate;
            return true;
        }

        correlationId = string.Empty;
        return false;
    }

    public static string NormalizeOrCreate(string? value, int maximumLength)
    {
        return TryNormalize(value, maximumLength, out var correlationId)
            ? correlationId
            : Guid.NewGuid().ToString("N");
    }

    private static bool IsAllowedCharacter(char character)
    {
        return character is >= 'a' and <= 'z' or
            >= 'A' and <= 'Z' or
            >= '0' and <= '9' or
            '-' or '_' or '.' or ':' or '/';
    }
}
