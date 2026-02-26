using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

public abstract class GenericConfigurator<T> : IGenericConfigurator<T> where T : class
{
    public Type Type => typeof(T);
    
    public abstract Type[] DependsOn { get; }

    public virtual void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<T>().ToTable(typeof(T).Name);
    }

    public virtual void Seed(ModelBuilder modelBuilder)
    {
    }
}