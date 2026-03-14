using System.Collections;
using System.Reflection;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Provides a default implementation of the <see cref="IExceptionDataMasker"/> interface for masking
/// exception data. This class is designed to process various data types, including primitive values,
/// collections, dictionaries, and complex objects, ensuring sensitive information is obscured
/// based on predefined masking rules.
/// </summary>
/// <remarks>
/// The <c>DefaultExceptionDataMasker</c> masks sensitive keys or properties with the value "***MASKED***".
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
public class DefaultExceptionDataMasker : IExceptionDataMasker
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Token", "Secret", "CardNumber", "CVV", "Pin", "AccessKey"
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
        if (data == null || data is string || data.GetType().IsValueType) return data;

        switch (data)
        {
            case IDictionary dictionary:
            {
                var maskedDict = new Dictionary<object, object?>();
                foreach (var key in dictionary.Keys)
                {
                    var keyStr = key.ToString();
                    maskedDict[key] = (keyStr != null && SensitiveKeys.Contains(keyStr)) 
                        ? "***MASKED***" 
                        : Mask(dictionary[key]);
                }
                return maskedDict;
            }
            case IEnumerable enumerable:
                return enumerable.Cast<object>().Select(Mask).ToList();
        }

        var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(data);
            result[prop.Name] = SensitiveKeys.Contains(prop.Name) ? "***MASKED***" : Mask(value);
        }

        return result;
    }
}