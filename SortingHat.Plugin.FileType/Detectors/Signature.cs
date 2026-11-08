using System.Collections.Generic;

namespace SortingHat.Plugin.FileType.Detectors;

/// <summary>What the first bytes of a file of one <see cref="FileType"/> look like.</summary>
internal abstract record Signature(FileType FileType)
{
    /// <summary>How many bytes from the start of a file <see cref="Matches"/> looks at.</summary>
    public abstract int HeadLength { get; }

    /// <summary>Whether a file starting with <paramref name="head"/> has this signature; <paramref name="head"/> is shorter than <see cref="HeadLength"/> when the file is.</summary>
    public abstract bool Matches(IReadOnlyList<byte> head);
}
