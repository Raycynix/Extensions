using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Masks sensitive values in exception-related data structures.
/// </summary>
public class ExceptionDataMasker : IExceptionDataMasker
{
    private const string MaskedValue = "***MASKED***";
    
    private static readonly HashSet<string> _sensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Token",
        "AccessToken",
        "RefreshToken",
        "Secret",
        "ClientSecret",
        "ApiKey",
        "AccessKey",
        "Authorization",
        "Cookie",
        "ConnectionString",
        "CardNumber",
        "CVV",
        "Pin",
        "PrivateKey"
    };

    /// <summary>
    /// Masks sensitive values in the supplied object graph.
    /// </summary>
    /// <param name="data">The data to sanitize.</param>
    /// <returns>A sanitized copy of the supplied data.</returns>
    public object? Mask(object? data)
    {
        return MaskInternal(data, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static object? MaskInternal(object? data, ISet<object> visited)
    {
        if (data is null) return null;

        var type = data.GetType();
        if (data is string || type.IsPrimitive || type.IsEnum || type.IsValueType)
        {
            return data;
        }

        if (!visited.Add(data))
        {
            return "[CircularReference]";
        }

        switch (data)
        {
            case IDictionary dictionary:
            {
                var maskedDict = new Dictionary<object, object?>();
                foreach (var key in dictionary.Keys)
                {
                    var keyName = key?.ToString();
                    maskedDict[key!] = keyName is not null && _sensitiveKeys.Contains(keyName)
                        ? MaskedValue
                        : MaskInternal(dictionary[key!], visited);
                }

                return maskedDict;
            }

            case IEnumerable enumerable:
            {
                var items = new List<object?>();
                foreach (var item in enumerable)
                {
                    items.Add(MaskInternal(item, visited));
                }

                return items;
            }
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        var result = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            object? value;

            try
            {
                value = prop.GetValue(data);
            }
            catch
            {
                value = "[Unavailable]";
            }

            result[prop.Name] = _sensitiveKeys.Contains(prop.Name)
                ? MaskedValue
                : MaskInternal(value, visited);
        }

        return result;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
