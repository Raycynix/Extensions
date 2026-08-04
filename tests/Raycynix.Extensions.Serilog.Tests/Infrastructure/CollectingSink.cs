using System.Collections.Concurrent;
using FluentAssertions;
using Serilog.Core;
using Serilog.Events;

namespace Raycynix.Extensions.Serilog.Tests.Infrastructure;

internal sealed class CollectingSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = [];

    public IReadOnlyCollection<LogEvent> Events => Snapshot();

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        _events.Enqueue(logEvent);
    }

    public IReadOnlyCollection<LogEvent> Snapshot()
    {
        return [.. _events];
    }

    public LogEvent Single(string messageTemplate)
    {
        var events = Snapshot();

        return events
            .Where(current => current.MessageTemplate.Text == messageTemplate)
            .Should()
            .ContainSingle(
                because: "the sink should contain exactly one event with template {0}. Actual templates: {1}",
                messageTemplate,
                string.Join(", ", events.Select(current => $"'{current.MessageTemplate.Text}'")))
            .Subject;
    }
}