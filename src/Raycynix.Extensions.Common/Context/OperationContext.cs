using System.Diagnostics;

namespace Raycynix.Extensions.Common.Context;

/// <summary>
/// 
/// </summary>
public interface IOperationContext
{
    /// <summary>
    /// 
    /// </summary>
    string CorrelationId { get; set; } //TODO: FIX SETTER

    /// <summary>
    /// 
    /// </summary>
    string TraceId { get; }
    
    /// <summary>
    /// 
    /// </summary>
    string? UserId { get; set;  }
}

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
public class OperationContext :  IOperationContext
{
    private static readonly AsyncLocal<string> _correlationId = new();
    private static readonly AsyncLocal<string>? _userId = new();

    /// <inheritdoc/>
    public string CorrelationId
    {
        get => _correlationId.Value ??=  Guid.NewGuid().ToString();
        set => _correlationId.Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public string TraceId => Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

    /// <inheritdoc/>
    public string? UserId
    {
        get => _userId?.Value;
        set => _userId?.Value = value ??= Guid.NewGuid().ToString();
    }
}