namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

internal static class AuthorizationRequirementValues
{
    public static string Required(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    public static IReadOnlyCollection<string> RequiredMany(
        IReadOnlyCollection<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        if (values.Count == 0)
        {
            throw new ArgumentException("At least one authorization value is required.", parameterName);
        }

        return values
            .Select(value => Required(value, parameterName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
