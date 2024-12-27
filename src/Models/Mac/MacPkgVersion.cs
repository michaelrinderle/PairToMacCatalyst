using System;

namespace Ptm.Models.Mac;

/// <summary>
///     Represents a version of a Mac package, including major, minor, build, revision, patch, and build metadata
///     components.
/// </summary>
public class MacPkgVersion : IComparable<MacPkgVersion>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MacPkgVersion" /> class.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    public MacPkgVersion(string version)
    {
        var versionParts = version.Split('.');

        Major = versionParts.Length > 0 ? int.Parse(versionParts[0]) : 0;
        Minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
        Build = versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0;
        Revision = versionParts.Length > 3 ? int.Parse(versionParts[3]) : null;
        Patch = versionParts.Length > 4 ? int.Parse(versionParts[4]) : null;
        BuildMetadata = versionParts.Length > 5 ? long.Parse(versionParts[5]) : null;
    }

    public int Major { get; set; }

    public int Minor { get; set; }

    public int Build { get; set; }

    public int? Revision { get; set; }

    public int? Patch { get; set; }

    public long? BuildMetadata { get; set; }

    /// <summary>
    ///     Compares the current instance with another object of the same type and returns an integer that indicates whether
    ///     the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(MacPkgVersion other)
    {
        if (other == null) return 1;

        var result = Major.CompareTo(other.Major);
        if (result != 0) return result;

        result = Minor.CompareTo(other.Minor);
        if (result != 0) return result;

        result = Build.CompareTo(other.Build);
        if (result != 0) return result;

        result = Nullable.Compare(Revision, other.Revision);
        if (result != 0) return result;

        result = Nullable.Compare(Patch, other.Patch);
        if (result != 0) return result;

        return Nullable.Compare(BuildMetadata, other.BuildMetadata);
    }

    /// <summary>
    ///     Determines whether one <see cref="MacPkgVersion" /> is less than another.
    /// </summary>
    /// <param name="left">The first <see cref="MacPkgVersion" /> to compare.</param>
    /// <param name="right">The second <see cref="MacPkgVersion" /> to compare.</param>
    /// <returns>True if the first <see cref="MacPkgVersion" /> is less than the second; otherwise, false.</returns>
    public static bool operator <(MacPkgVersion left, MacPkgVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Determines whether one <see cref="MacPkgVersion" /> is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="MacPkgVersion" /> to compare.</param>
    /// <param name="right">The second <see cref="MacPkgVersion" /> to compare.</param>
    /// <returns>True if the first <see cref="MacPkgVersion" /> is less than or equal to the second; otherwise, false.</returns>
    public static bool operator <=(MacPkgVersion left, MacPkgVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Determines whether one <see cref="MacPkgVersion" /> is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="MacPkgVersion" /> to compare.</param>
    /// <param name="right">The second <see cref="MacPkgVersion" /> to compare.</param>
    /// <returns>True if the first <see cref="MacPkgVersion" /> is greater than the second; otherwise, false.</returns>
    public static bool operator >(MacPkgVersion left, MacPkgVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Determines whether one <see cref="MacPkgVersion" /> is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="MacPkgVersion" /> to compare.</param>
    /// <param name="right">The second <see cref="MacPkgVersion" /> to compare.</param>
    /// <returns>True if the first <see cref="MacPkgVersion" /> is greater than or equal to the second; otherwise, false.</returns>
    public static bool operator >=(MacPkgVersion left, MacPkgVersion right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (obj is MacPkgVersion other) return this == other;

        return false;
    }

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hash = 17;
        hash = hash * 23 + Major.GetHashCode();
        hash = hash * 23 + Minor.GetHashCode();
        hash = hash * 23 + Build.GetHashCode();
        hash = hash * 23 + (Revision.HasValue ? Revision.Value.GetHashCode() : 0);
        hash = hash * 23 + (Patch.HasValue ? Patch.Value.GetHashCode() : 0);
        hash = hash * 23 + (BuildMetadata.HasValue ? BuildMetadata.Value.GetHashCode() : 0);
        return hash;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}" +
               $"{(Revision.HasValue ? "." + Revision.Value : string.Empty)}" +
               $"{(Patch.HasValue ? "." + Patch.Value : string.Empty)}" +
               $"{(BuildMetadata.HasValue ? "." + BuildMetadata.Value : string.Empty)}";
    }
}