namespace Raycynix.Extensions.Database.Models;

/// <summary>
/// Defines the supported database providers.
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
