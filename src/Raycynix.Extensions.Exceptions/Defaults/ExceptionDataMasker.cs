using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Provides a default implementation of the <see cref="IExceptionDataMasker"/> interface for masking
/// exception data. This class is designed to process various data types, including primitive values,
/// collections, dictionaries, and complex objects, ensuring sensitive information is obscured
/// based on predefined masking rules.
/// </summary>
/// <remarks>
/// The <c>ExceptionDataMasker</c> masks sensitive keys or properties with the value "***MASKED***".
/// It recursively processes objects, collections, and dictionaries, ensuring that all nested
/// data structures are appropriately masked.
/// </remarks>
/// <example>
/// The class handles the following scenarios:
/// - Primitive types and <c>null</c> are returned without modification.
/// - Dictionaries mask keys specified in the sensitive keys list.
/// - Collections are traversed, and their elements are recursively masked.
/// - Object properties are inspected, and sensitive properties are masked accordingly.
/// </example>
/// <seealso cref="IExceptionDataMasker"/>
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

    /// Masks sensitive data within the provided object according to predefined rules.
    /// <param name="data">
    /// The object containing data to be masked. It can be null, a primitive type, a string, a dictionary,
    /// a collection, or an object with public properties.
    /// </param>
    /// <return>
    /// A new object with sensitive information replaced by masked values. The type and structure of the returned
    /// object match the input, with sensitive data replaced by <b>"***MASKED***"</b> where applicable.
    /// </return>
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