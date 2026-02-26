using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.Implementation;

public class Tracer : ITracer
{
    private readonly ActivitySource _activitySource;

    public Tracer(string serviceName)
    {
        _activitySource = new ActivitySource(serviceName);
    }

    public IDisposable StartTrace(string name, Dictionary<string, string>? tags = null)
    {
        var activity = _activitySource.StartActivity(name);

        if (activity == null) return new NoopDisposable();
        if (tags == null) return activity;
        
        foreach (var tag in tags)
            activity.SetTag(tag.Key, tag.Value);

        return activity;
    }

    public void AddTag(string key, string value) => Activity.Current?.SetTag(key, value);

    public void SetBaggage(string key, string value) => Activity.Current?.SetBaggage(key, value);

    public string? GetBaggage(string key) => Activity.Current?.GetBaggageItem(key);

    private class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}