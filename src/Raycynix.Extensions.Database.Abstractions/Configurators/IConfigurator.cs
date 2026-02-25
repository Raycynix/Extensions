using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions.Configurators;

/// <summary>
/// 
/// </summary>
public interface IConfigurator
{
    /// <summary>
    /// 
    /// </summary>
    Type Type { get; }
    
    /// <summary>
    /// 
    /// </summary>
    Type[] DependsOn { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    void Configure(ModelBuilder modelBuilder);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    void Seed(ModelBuilder modelBuilder);
    
    //TODO: Create documentation
}