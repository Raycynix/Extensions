using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration;

/// <summary>
/// Builds configuration section paths from options type names.
/// </summary>
public static class ConfigurationSectionPath
{
    /// <summary>
    /// Gets the conventional configuration section name for an options type.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <returns>The options type name.</returns>
    public static string For<TOptions>()
        where TOptions : class
    {
        return typeof(TOptions).Name;
    }

    /// <summary>
    /// Builds a nested configuration path from parent and child options type names.
    /// </summary>
    /// <typeparam name="TParentOptions">The parent options type.</typeparam>
    /// <typeparam name="TOptions">The nested options type.</typeparam>
    /// <returns>A configuration path in <c>ParentOptions:NestedOptions</c> form.</returns>
    public static string Combine<TParentOptions, TOptions>()
        where TParentOptions : class
        where TOptions : class
    {
        return ConfigurationPath.Combine(
            For<TParentOptions>(),
            For<TOptions>());
    }
}
