using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database;

public static class Database
{
    public static IServiceCollection AddRaycynixDatabase(this IServiceCollection services, IConfiguration configuration,
        Action<DatabaseConfiguration>? setup = null)
    {
        var config = new DatabaseConfiguration();
        configuration.GetSection(nameof(DatabaseConfiguration)).Bind(config);

        setup?.Invoke(config);

        var finalString = ResolveConnection(config);
        var callerAssembly = Assembly.GetEntryAssembly()!;

        services.AddSingleton(config);
        services.AddSingleton(callerAssembly);
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        services.AddDbContextPool<DatabaseContext>(options =>
        {
            switch (config.Provider)
            {
                case DatabaseProvider.PostgreSql:
                    options.UseNpgsql(finalString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            config.RetryCount,
                            TimeSpan.FromSeconds(config.RetryDelaySeconds),
                            null);

                        npgsqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);
                    });
                    break;
                
                case DatabaseProvider.MsSqlServer:
                    options.UseSqlServer(finalString,
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                config.RetryCount,
                                TimeSpan.FromSeconds(config.RetryDelaySeconds),
                                null);

                            sqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);
                        });
                    break;
                
                case DatabaseProvider.MySql:
                    options.UseMySQL(finalString, mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            config.RetryCount,
                            TimeSpan.FromSeconds(config.RetryDelaySeconds),
                            null);

                        mySqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);
                    });
                    break;
                
                default:
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(finalString,
                        sqliteOptions => { sqliteOptions.MigrationsAssembly(callerAssembly.GetName().Name); });
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

                DatabaseProvider.MySql => $"Server={config.ConnectionConfiguration.Host};" +
                                          $"Port={config.ConnectionConfiguration.Port ?? 3306};" +
                                          $"Database={config.ConnectionConfiguration.Name};" +
                                          $"Uid={config.ConnectionConfiguration.Username};" +
                                          $"Pwd={config.ConnectionConfiguration.Password};" +
                                          $"AllowUserVariables=True;",

                DatabaseProvider.Sqlite => $"Data Source={config.ConnectionConfiguration.Name}.db",

                _ => throw new NotSupportedException($"Resolving for {config.Provider} not realised.")
            };
        }

        return config.ConnectionString ?? throw new ArgumentException("ConnectionString is missing.");
    }
}