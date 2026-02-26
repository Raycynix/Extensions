namespace Raycynix.Extensions.Database.Abstractions;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    bool IsReady { get; }
}