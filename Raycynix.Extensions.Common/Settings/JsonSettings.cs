using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Raycynix.Extensions.Common.Settings;

/// <summary>
/// Provides predefined and customizable settings for JSON serialization and deserialization.
/// </summary>
public static class JsonSettings
{
    /// Gets or sets the global default instance of JsonSerializerSettings used across the application for JSON serialization and deserialization.
    /// This property provides a pre-configured instance of JsonSerializerSettings with commonly used defaults, such as:
    /// - CamelCase naming for property resolution.
    /// - Ignoring null values during serialization to reduce JSON payloads.
    /// - Handling and skipping reference loops to prevent serialization errors.
    /// - Formatting dates in ISO 8601 standard for consistency.
    /// - Serializing enums as strings using StringEnumConverter to improve readability and compatibility.
    /// This property can be customized as needed but should generally be used as the baseline configuration for consistent JSON handling.
    public static JsonSerializerSettings Default { get; set; } = CreateDefaults();

    /// Creates and configures a new instance of JsonSerializerSettings with default settings optimized for most common use cases.
    /// The configuration includes:
    /// - Property names are resolved using camelCase.
    /// - Ignores null values during serialization to reduce payload size.
    /// - Prevents issues with circular references by ignoring reference loops during serialization.
    /// - Formats dates using ISO 8601 standard to ensure consistency and compatibility.
    /// - Includes support for serializing and deserializing enums as strings using StringEnumConverter.
    /// <returns>A pre-configured instance of JsonSerializerSettings with these settings applied.</returns>
    public static JsonSerializerSettings CreateDefaults() => new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        Converters = { new StringEnumConverter() }
    };
}