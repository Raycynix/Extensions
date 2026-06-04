using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions;

public interface IDatabaseModelConfigurator
{
    void Configure(ModelBuilder modelBuilder, string providerName);
    string GetModelCacheKey(string providerName);
}