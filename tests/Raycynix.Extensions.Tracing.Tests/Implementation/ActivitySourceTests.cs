using System.Diagnostics;
using FluentAssertions;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.Tests.Implementation;

/// <summary>
/// Covers the shared standard .NET activity source.
/// </summary>
public sealed class ActivitySourceTests
{
    [Fact]
    public void ActivitySource_ShouldUseStableIdentity()
    {
        RaycynixTracing.ActivitySource.Name.Should().Be(RaycynixTracing.SourceName);
        RaycynixTracing.ActivitySource.Version.Should().Be("3.0.0");
    }

    [Fact]
    public void StartActivity_ShouldCreateStandardActivity_WithTagsAndBaggage()
    {
        using var listener = CreateListener();

        using var activity = RaycynixTracing.ActivitySource.StartActivity("catalog.read", ActivityKind.Internal);
        activity?.SetTag("raycynix.tenant", "alpha");
        activity?.SetBaggage("raycynix.region", "eu");

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("catalog.read");
        activity.Source.Should().BeSameAs(RaycynixTracing.ActivitySource);
        activity.GetTagItem("raycynix.tenant").Should().Be("alpha");
        activity.GetBaggageItem("raycynix.region").Should().Be("eu");
    }

    [Fact]
    public void StartActivity_ShouldReturnNull_WhenNoListenerExists()
    {
        using var isolatedSource = new ActivitySource("raycynix.tests.no-listener");

        isolatedSource.StartActivity("noop").Should().BeNull();
    }

    private static ActivityListener CreateListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == RaycynixTracing.SourceName,
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
