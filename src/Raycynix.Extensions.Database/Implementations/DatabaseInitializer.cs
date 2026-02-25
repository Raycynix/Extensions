
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// 
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    public DatabaseInitializer(IServiceProvider serviceProvider, DatabaseConfiguration config, ILogger<DatabaseInitializer> logger)
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    //TODO: Create Documentation
}