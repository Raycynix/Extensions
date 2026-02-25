namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// 
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    bool IsReady { get; }
    
    //TODO: Create Documentation
}