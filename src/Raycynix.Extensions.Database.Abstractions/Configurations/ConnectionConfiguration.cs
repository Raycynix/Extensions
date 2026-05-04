namespace Raycynix.Extensions.Database.Abstractions.Configurations;

/// <summary>
/// Represents the base connection settings used to build a database connection string.
/// </summary>
public class ConnectionConfiguration
{
    /// <summary>
    /// Gets the host name or IP address of the database server.
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// Gets the database server port.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the database name or data source name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the username used to authenticate the connection.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets the password used to authenticate the connection.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Validates that the configuration contains the minimum required values.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    public virtual void Validate(string providerName)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException($"{providerName} connection requires a database name.");
        }
    }
}
