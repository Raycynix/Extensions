using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database.Configurations;

public class DatabaseConfiguration
{
    public string? ConnectionString { get; init; }
    
    public ConnectionConfiguration? ConnectionConfiguration { get; init; }
    
    public DatabaseProvider Provider { get; init; } = DatabaseProvider.PostgreSql;

    public bool UseMigrations { get; init; } = false;
    
    public bool EnsureCreated { get; init; } = true;
    
    public bool EnableSeed { get; init; } = true;

    public int RetryCount { get; init; } = 5;
    
    public int RetryDelaySeconds { get; init; } = 10;
    
    public PostgreSqlConfiguration? PostgreSqlConfiguration { get; init; }
    
    public MsSqlServerConfiguration? MsSqlServerConfiguration { get; init; }
    
    public MySqlConfiguration? MySqlConfiguration { get; init; }
    
    public SqlliteConfiguration? SqlliteConfiguration { get; init; }
}