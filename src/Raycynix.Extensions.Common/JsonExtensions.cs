using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Raycynix.Extensions.Common.Settings;

namespace Raycynix.Extensions.Common;

/// <summary>
/// Provides extension methods for JSON serialization and deserialization using preconfigured settings.
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Converts an object to its JSON string representation using preconfigured JSON serialization settings.
    /// </summary>
    /// <param name="obj">The object to serialize. If null, an empty string is returned.</param>
    /// <param name="indented">
    /// A boolean indicating whether the JSON output should be indented for readability.
    /// If true, the output will be formatted with indentation; otherwise, it will not.
    /// </param>
    /// <returns>A JSON-formatted string representation of the object.</returns>
    public static string ToJson(this object? obj, bool indented = false)
    {
        if (obj is null) return string.Empty;

        var settings = JsonSettings.Default;
        if (!indented) return JsonConvert.SerializeObject(obj, settings);

        settings = new JsonSerializerSettings();
        JsonConvert.PopulateObject(JsonConvert.SerializeObject(JsonSettings.Default), settings);
        settings.Formatting = Formatting.Indented;

        return JsonConvert.SerializeObject(obj, settings);
    }

    /// <summary>
    /// Deserializes a JSON string into an object of the specified type using preconfigured JSON deserialization settings.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. If null or whitespace, the method returns the default value for the type.</param>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <returns>An object of type <c>T</c> deserialized from the JSON string, or the default value for the type if the input is invalid.</returns>
    public static T? FromJson<T>(this string? json)
    {
        return string.IsNullOrWhiteSpace(json) ? default : JsonConvert.DeserializeObject<T>(json, JsonSettings.Default);
    }

    /// <summary>
    /// Creates a deep copy of the specified object by serializing it to JSON and then deserializing it back into an object.
    /// </summary>
    /// <typeparam name="T">The type of the object to be cloned.</typeparam>
    /// <param name="obj">The object to clone. If null, the method returns the default value for the type.</param>
    /// <returns>A new instance of the object that is a deep copy of the original, or the default value if the input is null.</returns>
    public static T? DeepClone<T>(this T obj)
    {
        return obj is null ? default : obj.ToJson().FromJson<T>();
    }
}