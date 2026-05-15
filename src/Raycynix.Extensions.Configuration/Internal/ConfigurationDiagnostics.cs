using System.Collections;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Configuration.Configurations;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class ConfigurationDiagnostics(
    ConfigurationDiagnosticsStore store,
    IServiceProvider serviceProvider,
    IConfigurationRedactor redactor,
    ConfigurationDiagnosticsOptions options)
    : IConfigurationDiagnostics
{
    public IReadOnlyCollection<ConfigurationRegistrationInfo> GetRegistrations()
    {
        return store.GetRegistrations();
    }

    public IReadOnlyCollection<ConfigurationReloadInfo> GetReloads()
    {
        return store.GetReloads();
    }

    public object? GetRedactedSnapshot<TOptions>(string? optionsName = null)
        where TOptions : class
    {
        if (!options.EnableSnapshots)
        {
            throw new InvalidOperationException("Configuration snapshots are disabled.");
        }

        var accessor = serviceProvider.GetRequiredService<IConfigurationAccessor<TOptions>>();
        var snapshot = accessor.Get(optionsName);

        return RedactValue(string.Empty, snapshot, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private object? RedactValue(string key, object? value, ISet<object> visited)
    {
        var redacted = redactor.Redact(key, value);
        if (!ReferenceEquals(redacted, value))
        {
            return redacted;
        }

        if (value is null || IsScalar(value.GetType()))
        {
            return value;
        }

        if (!visited.Add(value))
        {
            return null;
        }

        if (value is IDictionary dictionary)
        {
            return RedactDictionary(key, dictionary, visited);
        }

        if (value is IEnumerable enumerable and not string)
        {
            return RedactEnumerable(key, enumerable, visited);
        }

        return RedactObject(value, visited);
    }

    private Dictionary<string, object?> RedactDictionary(string parentKey, IDictionary dictionary, ISet<object> visited)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (DictionaryEntry entry in dictionary)
        {
            var key = entry.Key?.ToString() ?? string.Empty;
            result[key] = RedactValue(JoinKey(parentKey, key), entry.Value, visited);
        }

        return result;
    }

    private List<object?> RedactEnumerable(string key, IEnumerable enumerable, ISet<object> visited)
    {
        var result = new List<object?>();

        foreach (var item in enumerable)
        {
            result.Add(RedactValue(key, item, visited));
        }

        return result;
    }

    private Dictionary<string, object?> RedactObject(object value, ISet<object> visited)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var properties = value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            result[property.Name] = RedactValue(property.Name, property.GetValue(value), visited);
        }

        return result;
    }

    private static bool IsScalar(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive ||
               underlyingType.IsEnum ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid) ||
               underlyingType == typeof(Uri);
    }

    private static string JoinKey(string parentKey, string key)
    {
        return string.IsNullOrWhiteSpace(parentKey)
            ? key
            : $"{parentKey}.{key}";
    }
}
