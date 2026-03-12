using System.Collections;
using System.Reflection;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

public class DefaultExceptionDataMasker : IExceptionDataMasker
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Token", "Secret", "CardNumber", "CVV", "Pin", "AccessKey"
    };
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