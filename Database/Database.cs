using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementation;
using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database;

public static class Database
{
    public static IServiceCollection AddUniversalDatabase(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Action<DatabaseConfiguration>? setup = null)
    {
        var config = new DatabaseConfiguration();
        configuration.GetSection(nameof(DatabaseConfiguration)).Bind(config);

        setup?.Invoke(config);

        var finalString = ResolveConnection(config);
        var callerAssembly = Assembly.GetCallingAssembly();

        serviceCollection.AddSingleton(config);
        serviceCollection.AddSingleton(callerAssembly);
        serviceCollection.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        serviceCollection.AddDbContext<DatabaseContext>(options =>
        {
            switch (config.Provider)
            {
                case DatabaseProvider.PostgreSQL:
                    //TODO: Create providing to PostgreSQL using NpgSql;
                    break;
                case DatabaseProvider.MsSQLServer:
                    //TODO: Create providing to MsSQL Server;
                    break;
                case DatabaseProvider.MySQL:
                    //TODO: Create providing to MySQL;
                    break;
                default:
                case DatabaseProvider.Sqlite:
                    //TODO: Create providing to Sqlite;
                    break;
            }
        });
    }

    private static string ResolveConnection(DatabaseConfiguration config)
    {
        if (config.ConnectionConfiguration != null && !string.IsNullOrEmpty(config.ConnectionConfiguration.Host))
        {
            return config.Provider switch
            {
                DatabaseProvider.PostgreSQL => $"Host={config.ConnectionConfiguration.Host};" +
                                               $"Port={config.ConnectionConfiguration.Port ?? 5432};" +
                                               $"Database={config.ConnectionConfiguration.Name};" +
                                               $"Username={config.ConnectionConfiguration.Username};" +
                                               $"Password={config.ConnectionConfiguration.Password};",
                DatabaseProvider.MsSQLServer => $"Server={config.ConnectionConfiguration.Host};" +
                                                $"Database={config.ConnectionConfiguration.Name};" +
                                                $"User Id={config.ConnectionConfiguration.Username};" +
                                                $"Password={config.ConnectionConfiguration.Password};" +
                                                $"TrustServerCertificate=True;",
                DatabaseProvider.Sqlite => "",
                DatabaseProvider.MySQL => "",
                _ => throw new NotSupportedException($"Resolving for {config.Provider} not realised.")
            };
        }

        return config.ConnectionString ?? throw new ArgumentException("ConnectionString is missing.");
    }
}