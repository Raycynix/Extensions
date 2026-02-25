namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// 
/// </summary>
public class ConnectionConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public string? Host { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public int? Port { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string? Password { get; set; }
    
    //TODO: Create Documentation
}