using System.Globalization;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a semantic contract version.
/// </summary>
public class ContractVersion : IComparable<ContractVersion>, IEquatable<ContractVersion>
{
    /// <summary>
    /// Represents the default initial contract version.
    /// </summary>
    public static ContractVersion Initial { get; } = new();

    /// <summary>
    /// Gets or sets the major version.
    /// </summary>
    public int Major { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minor version.
    /// </summary>
    public int Minor { get; set; }

    /// <summary>
    /// Gets or sets the patch version.
    /// </summary>
    public int Patch { get; set; }

    /// <summary>
    /// Parses a semantic contract version string.
    /// </summary>
    /// <param name="value">The version string in major.minor.patch format.</param>
    /// <returns>The parsed contract version.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown when the value is not a valid semantic version.</exception>
    public static ContractVersion Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Version value cannot be null or whitespace.", nameof(value));
        }

        if (!TryParse(value, out ContractVersion? version))
        {
            throw new FormatException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Value '{0}' is not a valid contract version. Expected format: major.minor.patch with non-negative integers.",
                    value));
        }

        return version!;
    }

    /// <summary>
    /// Attempts to parse a semantic contract version string.
    /// </summary>
    /// <param name="value">The version string in major.minor.patch format.</param>
    /// <param name="version">The parsed contract version when successful.</param>
    /// <returns><c>true</c> when parsing succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? value, out ContractVersion? version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] segments = value.Split('.', StringSplitOptions.TrimEntries);
        if (segments.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(segments[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major) ||
            !int.TryParse(segments[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor) ||
            !int.TryParse(segments[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch))
        {
            return false;
        }

        if (major < 0 || minor < 0 || patch < 0)
        {
            return false;
        }

        version = new ContractVersion
        {
            Major = major,
            Minor = minor,
            Patch = patch
        };

        return true;
    }

    /// <summary>
    /// Determines whether the current version contains only valid non-negative components.
    /// </summary>
    /// <returns><c>true</c> when the version is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return Major >= 0 && Minor >= 0 && Patch >= 0;
    }

    /// <summary>
    /// Compares the current version to another version.
    /// </summary>
    /// <param name="other">The other version.</param>
    /// <returns>A value indicating relative ordering.</returns>
    public int CompareTo(ContractVersion? other)
    {
        if (ReferenceEquals(other, null))
        {
            return 1;
        }

        int majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
        {
            return majorComparison;
        }

        int minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0)
        {
            return minorComparison;
        }

        return Patch.CompareTo(other.Patch);
    }

    /// <inheritdoc />
    public bool Equals(ContractVersion? other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ContractVersion other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch);
    }

    /// <summary>
    /// Determines whether one version is less than another.
    /// </summary>
    public static bool operator <(ContractVersion? left, ContractVersion? right)
    {
        return Compare(left, right) < 0;
    }

    /// <summary>
    /// Determines whether one version is less than or equal to another.
    /// </summary>
    public static bool operator <=(ContractVersion? left, ContractVersion? right)
    {
        return Compare(left, right) <= 0;
    }

    /// <summary>
    /// Determines whether one version is greater than another.
    /// </summary>
    public static bool operator >(ContractVersion? left, ContractVersion? right)
    {
        return Compare(left, right) > 0;
    }

    /// <summary>
    /// Determines whether one version is greater than or equal to another.
    /// </summary>
    public static bool operator >=(ContractVersion? left, ContractVersion? right)
    {
        return Compare(left, right) >= 0;
    }

    /// <summary>
    /// Determines whether two versions are equal.
    /// </summary>
    public static bool operator ==(ContractVersion? left, ContractVersion? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two versions are not equal.
    /// </summary>
    public static bool operator !=(ContractVersion? left, ContractVersion? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Returns the version as a semantic version string.
    /// </summary>
    /// <returns>The semantic version string.</returns>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    private static int Compare(ContractVersion? left, ContractVersion? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (ReferenceEquals(left, null))
        {
            return -1;
        }

        return left.CompareTo(right);
    }
}
