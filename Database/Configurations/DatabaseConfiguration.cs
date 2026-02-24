using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database.Configurations;

public class DatabaseConfiguration
{
    public string? ConnectionString { get; set; }
    public ConnectionConfiguration? ConnectionConfiguration { get; set; }
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.PostgreSQL;

    public bool UseMigrations { get; set; } = false;
    public bool EnsureCreated { get; set; } = true;
    public bool EnableSeed { get; set; } = true;

    public int RetryCount { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 3;
    
    //TODO: Add ProviderOptions for every database
}