using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
///

//TODO: Create Documentation
public abstract class GenericConfigurator<T> : IGenericConfigurator<T> where T : class
{
    /// <summary>
    /// 
    /// </summary>
    public Type Type => typeof(T);
    
    /// <summary>
    /// 
    /// </summary>
    public abstract Type[] DependsOn { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    public virtual void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<T>().ToTable(typeof(T).Name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    public virtual void Seed(ModelBuilder modelBuilder)
    {
    }
}