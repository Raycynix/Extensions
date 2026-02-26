using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database;

public static class Database
{
    public static IServiceCollection AddRaycynixDatabase(this IServiceCollection services, IConfiguration configuration, Action<DatabaseConfiguration>? setup = null)
    {
        var config = new DatabaseConfiguration();
        configuration.GetSection(nameof(DatabaseConfiguration)).Bind(config);

        setup?.Invoke(config);

        var finalString = ResolveConnection(config);
        var callerAssembly = Assembly.GetCallingAssembly();

        services.AddSingleton(config);
        services.AddSingleton(callerAssembly);
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        services.AddDbContext<DatabaseContext>(options =>
        {
            switch (config.Provider)
            {
                case DatabaseProvider.PostgreSql:
                    //TODO: Create providing to PostgreSQL using NpgSql;
                    break;
                case DatabaseProvider.MsSqlServer:
                    //TODO: Create providing to MsSQL Server;
                    break;
                case DatabaseProvider.MySql:
                    //TODO: Create providing to MySQL;
                    break;
                default:
                case DatabaseProvider.Sqlite:
                    //TODO: Create providing to Sqlite;
                    break;
            }
        });

        return services;
    }

    private static string ResolveConnection(DatabaseConfiguration config)
    {
        if (config.ConnectionConfiguration != null && !string.IsNullOrEmpty(config.ConnectionConfiguration.Host))
        {
            return config.Provider switch
            {
                DatabaseProvider.PostgreSql => $"Host={config.ConnectionConfiguration.Host};" +
                                               $"Port={config.ConnectionConfiguration.Port ?? 5432};" +
                                               $"Database={config.ConnectionConfiguration.Name};" +
                                               $"Username={config.ConnectionConfiguration.Username};" +
                                               $"Password={config.ConnectionConfiguration.Password};",
                
                DatabaseProvider.MsSqlServer => $"Server={config.ConnectionConfiguration.Host};" +
                                                $"Database={config.ConnectionConfiguration.Name};" +
                                                $"User Id={config.ConnectionConfiguration.Username};" +
                                                $"Password={config.ConnectionConfiguration.Password};" +
                                                $"TrustServerCertificate=True;",
                
                DatabaseProvider.Sqlite => "",
                
                DatabaseProvider.MySql => "",
                _ => throw new NotSupportedException($"Resolving for {config.Provider} not realised.")
            };
        }

        return config.ConnectionString ?? throw new ArgumentException("ConnectionString is missing.");
    }
}