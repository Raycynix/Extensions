using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Database;

public static class Database
{
    public static IServiceCollection AddRaycynixDatabase(this IServiceCollection services)
    {
        return services;
    }
}