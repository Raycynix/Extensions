using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementation;

public class DatabaseInitializer : IDatabaseInitializer
{
    public bool IsReady { get; private set; }

    public DatabaseInitializer(IServiceProvider serviceProvider, DatabaseConfiguration config, ILogger logger)
    {
        
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}