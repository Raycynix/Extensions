namespace Raycynix.Extensions.Tracing.Abstractions;

public interface ITracer
{
    IDisposable StartTrace(string name, Dictionary<string, string>? tags = null);

    void AddTag(string key, string value);
    
    void SetBaggage(string key, string value);
    
    string? GetBaggage(string key);
}