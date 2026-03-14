namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents a configuration for establishing a database connection.
/// This abstract class provides properties to define fundamental connection details
/// such as the host, port, database name, and credentials.
/// </summary>
public abstract class ConnectionConfiguration
{
    /// Gets or sets the host name or IP address of the database server.
    public string? Host { get; init; }

    /// Gets or sets the port number.
    public int? Port { get; init; }

    /// Gets the name of the database to which the connection will be established.
    /// This property is used when constructing the connection string for the database.
    /// The value of this property should typically represent the identifier of the database
    /// within the targeted database server or system.
    public string? Name { get; init; }

    /// Gets the username used for authentication with the database connection.
    /// This property represents the user identifier required to establish a connection
    /// to the database, as part of the database connection configuration. It is typically
    /// used in conjunction with the password.
    /// The value of this property is optional and may be null, depending on the specific database
    /// requirements and the connection configuration being utilized.
    public string? Username { get; init; }

    /// <summary>
    /// Gets or initializes the password used for authenticating the connection to the database.
    /// </summary>
    /// <remarks>
    /// This property is part of the connection configuration and is typically used in conjunction
    /// with the <see cref="Username"/> property to authenticate with the database server.
    /// Ensure that passwords are securely handled and not exposed in plain text.
    /// </remarks>
    public string? Password { get; init; }
}