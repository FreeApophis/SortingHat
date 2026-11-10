using System;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>
/// What hat knows about a path: nothing (<see cref="New"/>), its content (<see cref="Known"/>), or content the file no longer has (<see cref="Changed"/>).
/// </summary>
public closed record PathState
{
    /// <summary>The file at the path as it is now: the one to store or tag.</summary>
    public abstract FileAt Current { get; }

    /// <summary>
    /// The <paramref name="stored"/> file is trusted without reading the file again, as long as its fingerprint still matches the file on disk.
    /// </summary>
    /// <param name="stored">What the database knows about the path.</param>
    /// <param name="size">The size of the file on disk.</param>
    /// <param name="modifiedAtUtc">The last write time of the file on disk.</param>
    /// <returns>The path is known; none when the file must be hashed to tell.</returns>
    public static Option<PathState> Trust(Option<FileAt> stored, long size, DateTime modifiedAtUtc)
        => from file in stored
           from fingerprint in file.Fingerprint
           where fingerprint.StillMatches(size, modifiedAtUtc)
           select (PathState)new Known(file);

    /// <summary>Compares what the database knows about the path with the file hashed from disk.</summary>
    /// <param name="stored">What the database knows about the path.</param>
    /// <param name="current">The file at the path, hashed from disk.</param>
    /// <returns>What hat knows about the path.</returns>
    public static PathState Decide(Option<FileAt> stored, FileAt current)
        => stored.Match<PathState>(
            none: () => new New(current),
            some: file => file.File.Hash == current.File.Hash
                ? new Known(file with { Fingerprint = current.Fingerprint })
                : new Changed(file, current));

    /// <summary>The database does not know the path. The content may still be known at other paths.</summary>
    /// <param name="Current">The file at the path, hashed from disk.</param>
    public sealed record New(FileAt Current) : PathState
    {
        public override FileAt Current { get; } = Current;
    }

    /// <summary>The path still has the content the database knows for it.</summary>
    /// <param name="Current">The stored file, with the newest fingerprint.</param>
    public sealed record Known(FileAt Current) : PathState
    {
        public override FileAt Current { get; } = Current;
    }

    /// <summary>The file at the path was edited: the database knows the content it had before.</summary>
    /// <param name="Stored">What the database knows about the path, the content before the change.</param>
    /// <param name="Current">The file at the path, hashed from disk.</param>
    public sealed record Changed(FileAt Stored, FileAt Current) : PathState
    {
        public override FileAt Current { get; } = Current;
    }
}
