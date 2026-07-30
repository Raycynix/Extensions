using FluentAssertions;
using FluentAssertions.Extensibility;

[assembly: AssertionEngineInitializer(
    typeof(FluentAssertionsInitializer),
    nameof(FluentAssertionsInitializer.Initialize))]

internal static class FluentAssertionsInitializer
{
    public static void Initialize()
    {
        License.Accepted = true;
    }
}
