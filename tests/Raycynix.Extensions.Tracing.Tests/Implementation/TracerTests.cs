using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Implementations;

namespace Raycynix.Extensions.Tracing.Tests.Implementation;

/// <summary>
/// Covers the tracing abstraction built on top of diagnostic activities.
/// </summary>
public sealed class TracerTests
{
    /// <summary>
    /// Verifies that tracing registration exposes the shared tracer abstraction.
    /// </summary>
    [Fact]
    public void AddRaycynixTracing_ShouldRegisterTracer()
    {
        var services = new ServiceCollection();

        services.AddRaycynixTracing();

        var descriptor = services.Should()
            .ContainSingle(service => service.ServiceType == typeof(ITracer))
            .Subject;

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that started traces become activities and receive configured tags.
    /// </summary>
    [Fact]
    public void StartTrace_ShouldCreateActivity_AndApplyTags()
    {
        var tracer = new Tracer("tests.tracing");
        using var listener = CreateListener();

        using (tracer.StartTrace("catalog.read", new Dictionary<string, string>
               {
                   ["tenant"] = "alpha"
               }))
        {
            Activity.Current.Should().NotBeNull();
            Activity.Current!.OperationName.Should().Be("catalog.read");
            Activity.Current.GetTagItem("tenant").Should().Be("alpha");
        }

        Activity.Current.Should().BeNull();
    }

    /// <summary>
    /// Verifies that baggage and tags can be added to the current activity.
    /// </summary>
    [Fact]
    public void AddTagAndBaggage_ShouldModifyCurrentActivity()
    {
        var tracer = new Tracer("tests.tracing");
        using var listener = CreateListener();

        using (tracer.StartTrace("checkout"))
        {
            tracer.AddTag("order.id", "42");
            tracer.SetBaggage("tenant", "alpha");

            Activity.Current!.GetTagItem("order.id").Should().Be("42");
            tracer.GetBaggage("tenant").Should().Be("alpha");
        }
    }

    /// <summary>
    /// Verifies that starting a trace without listeners still returns a safe disposable handle.
    /// </summary>
    [Fact]
    public void StartTrace_ShouldReturnNoopDisposable_WhenNoListenerExists()
    {
        var tracer = new Tracer("tests.tracing.noop");

        var act = () =>
        {
            using var scope = tracer.StartTrace("noop");
        };

        act.Should().NotThrow();
        Activity.Current.Should().BeNull();
    }

    private static ActivityListener CreateListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = static _ => true,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
