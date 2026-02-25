using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// 
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public string? ConnectionString { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public ConnectionConfiguration? ConnectionConfiguration { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public DatabaseProvider Provider { get; init; } = DatabaseProvider.PostgreSql;

    /// <summary>
    /// 
    /// </summary>
    public bool UseMigrations { get; init; } = false;
    
    /// <summary>
    /// 
    /// </summary>
    public bool EnsureCreated { get; init; } = true;
    
    /// <summary>
    /// 
    /// </summary>
    public bool EnableSeed { get; init; } = true;

    /// <summary>
    /// 
    /// </summary>
    public int RetryCount { get; init; } = 5;
    
    /// <summary>
    /// 
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 3;
    
    //TODO: Add ProviderOptions for every database
    //TODO: Create Documentation
}