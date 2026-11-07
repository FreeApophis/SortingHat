using System;
using Funcky.Monads;
using SortingHat.API.Models;
using Xunit;

namespace SortingHat.Test;

/// <summary>What hat knows about a path: new, known or changed. Pure decisions, no database and no file system.</summary>
public sealed class PathStateTest
{
    private const string Path = @"C:\Documents\a.txt";

    private static readonly DateTime Written = new(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);

    private static readonly PathFingerprint Fingerprint = new(100, Written, Written.AddMinutes(1));

    private static readonly File Stored = new(Hash("sha256:" + new string('a', 64)), 100, Written.AddDays(-10));

    private static readonly File Edited = new(Hash("sha256:" + new string('b', 64)), 120, Written);

    [Fact]
    public void GivenNothingStoredForThePathThenItIsNew()
    {
        var current = new FileAt(Edited, Path, Fingerprint);

        Assert.Equal(new PathState.New(current), PathState.Decide(Option<FileAt>.None, current));
    }

    [Fact]
    public void GivenTheSameContentIsStoredForThePathThenItIsKnownWithTheStoredFileAndTheFreshFingerprint()
    {
        var current = new FileAt(Stored with { CreatedAt = Written }, Path, Fingerprint);

        var state = PathState.Decide(new FileAt(Stored, Path, Option<PathFingerprint>.None), current);

        Assert.Equal(new PathState.Known(new FileAt(Stored, Path, Fingerprint)), state);
    }

    [Fact]
    public void GivenOtherContentIsStoredForThePathThenItHasChanged()
    {
        var stored = new FileAt(Stored, Path, Option<PathFingerprint>.None);
        var current = new FileAt(Edited, Path, Fingerprint);

        Assert.Equal(new PathState.Changed(stored, current), PathState.Decide(stored, current));
    }

    [Fact]
    public void GivenTheStoredFingerprintStillMatchesThenTheStoredFileIsTrustedWithoutHashing()
    {
        var stored = new FileAt(Stored, Path, Fingerprint);

        Assert.Equal(Option.Some<PathState>(new PathState.Known(stored)), PathState.Trust(stored, Fingerprint.Size, Fingerprint.ModifiedAtUtc));
    }

    [Fact]
    public void GivenTheFileOnDiskNoLongerMatchesTheFingerprintThenItMustBeHashed()
        => Assert.Equal(Option<PathState>.None, PathState.Trust(new FileAt(Stored, Path, Fingerprint), Fingerprint.Size + 1, Fingerprint.ModifiedAtUtc));

    [Fact]
    public void GivenAStoredPathWithoutFingerprintThenItMustBeHashed()
        => Assert.Equal(Option<PathState>.None, PathState.Trust(new FileAt(Stored, Path, Option<PathFingerprint>.None), Fingerprint.Size, Fingerprint.ModifiedAtUtc));

    [Fact]
    public void GivenNothingStoredThenItMustBeHashed()
        => Assert.Equal(Option<PathState>.None, PathState.Trust(Option<FileAt>.None, Fingerprint.Size, Fingerprint.ModifiedAtUtc));

    [Fact]
    public void GivenAnyStateThenCurrentIsTheFileAtThePathToWorkWith()
    {
        var stored = new FileAt(Stored, Path, Option<PathFingerprint>.None);
        var current = new FileAt(Edited, Path, Fingerprint);

        Assert.Equal(current, PathState.Decide(stored, current).Current);
    }

    private static Hash Hash(string text)
        => API.Models.Hash.Parse(text).GetOrElse(problem => throw new InvalidOperationException(problem));
}
