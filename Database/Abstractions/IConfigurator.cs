using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions;

public interface IConfigurator
{
    Type Type { get; }
    Type[] DependsOn { get; }
    void Configure(ModelBuilder modelbuilder);
    void Seed(ModelBuilder modelbuilder);
}