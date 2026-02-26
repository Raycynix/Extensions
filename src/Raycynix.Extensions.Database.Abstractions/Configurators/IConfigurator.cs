using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions.Configurators;

public interface IConfigurator
{
    Type Type { get; }
    
    Type[] DependsOn { get; }
    
    void Configure(ModelBuilder modelBuilder);
    
    void Seed(ModelBuilder modelBuilder);
}