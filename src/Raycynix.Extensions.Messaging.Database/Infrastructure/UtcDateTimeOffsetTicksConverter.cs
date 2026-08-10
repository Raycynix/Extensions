using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Raycynix.Extensions.Messaging.Database.Infrastructure;

/// <summary>
/// Persists timestamps as UTC ticks so comparisons and ordering have identical semantics across providers.
/// </summary>
internal sealed class UtcDateTimeOffsetTicksConverter()
    : ValueConverter<DateTimeOffset, long>(
        value => value.UtcTicks,
        value => new DateTimeOffset(value, TimeSpan.Zero));
