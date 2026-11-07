using System;

namespace SortingHat.API.Models;

/// <summary>
/// The size and last write time of the file at a path, taken right before its content was hashed.
/// As long as the file still has that size and last write time, the stored hash still belongs to it, so hat does not read the file again.
/// </summary>
/// <param name="Size">The size in bytes.</param>
/// <param name="ModifiedAtUtc">The last write time.</param>
/// <param name="TakenAtUtc">When the fingerprint was taken.</param>
public sealed record PathFingerprint(long Size, DateTime ModifiedAtUtc, DateTime TakenAtUtc)
{
    /// <summary>The coarsest timestamp resolution of the usual file systems (FAT); a write within it can keep the timestamp.</summary>
    public static readonly TimeSpan TimestampResolution = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Whether a file with this <paramref name="size"/> and last write time is still the file that was hashed.
    /// A fingerprint taken right after a write is never trusted, because a second write could have kept the timestamp.
    /// </summary>
    public bool StillMatches(long size, DateTime modifiedAtUtc)
        => Size == size
            && ModifiedAtUtc == modifiedAtUtc
            && TakenAtUtc - ModifiedAtUtc >= TimestampResolution;
}
