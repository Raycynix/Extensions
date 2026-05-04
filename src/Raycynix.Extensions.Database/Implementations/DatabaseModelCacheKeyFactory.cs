using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Builds EF Core model cache keys that include dynamically registered configurator assemblies.
/// </summary>
internal sealed class DatabaseModelCacheKeyFactory : IModelCacheKeyFactory
{
    /// <inheritdoc />
    public object Create(DbContext context, bool designTime)
    {
        if (context is not RaycynixDatabaseContext databaseContext)
        {
            return (context.GetType(), designTime);
        }

        return (context.GetType(), databaseContext.GetModelCacheKey(), designTime);
    }
}
