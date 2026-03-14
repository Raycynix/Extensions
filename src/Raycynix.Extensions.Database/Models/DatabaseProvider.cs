namespace Raycynix.Extensions.Database.Models;

/// <summary>
/// Represents the PostgreSQL database provider for configuring a database connection.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Specifies PostgreSQL as the database provider.
    /// </summary>
    PostgreSql,

    /// <summary>
    /// Specifies Microsoft SQL Server as the database provider.
    /// </summary>
    MsSqlServer,

    /// <summary>
    /// Specifies SQLite as the database provider.
    /// </summary>
    Sqlite,

    /// <summary>
    /// Specifies MySQL as the database provider.
    /// </summary>
    MySql
}