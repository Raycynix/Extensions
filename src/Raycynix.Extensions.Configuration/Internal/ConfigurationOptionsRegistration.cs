namespace Raycynix.Extensions.Configuration.Internal;

internal sealed record ConfigurationOptionsRegistration<TOptions>(
    string Name,
    string SectionName,
    bool RequiredSection)
    where TOptions : class;