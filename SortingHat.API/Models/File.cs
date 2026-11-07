using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.API.Models;

[method: UsedImplicitly]
public class File(IFile file, IHashService hashService)
{
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }
    public Task<string> Hash { get; set; } = Task.FromResult("BADHASH");
    public string Path { get; set; } = "BADPATH";

    /// <summary>The fingerprint of <see cref="Path"/> when <see cref="Hash"/> was computed, when known.</summary>
    public Option<PathFingerprint> Fingerprint { get; set; }

    /// <summary>The hash stored for <see cref="Path"/>, when the file was hashed again because it might have changed.</summary>
    public Option<Task<string>> PreviousHash { get; private set; }

    /// <summary>Whether the file at <see cref="Path"/> was hashed again and its content differs from the stored one.</summary>
    public Task<bool> HasChanged()
        => PreviousHash.Match(none: () => Task.FromResult(false), some: async previous => await previous != await Hash);

    /// <summary>Loads what the database knows about <paramref name="filePath"/>.</summary>
    /// <returns>Whether the database knows the path; when it does not, nothing is loaded.</returns>
    public bool LoadByPathFromDb(string filePath)
    {
        Path = filePath;

        return file.LoadByPath(this);
    }

    /// <summary>Hashes the file at <paramref name="filePath"/>, its fingerprint is taken right before.</summary>
    public void LoadByPath(string filePath)
    {
        Path = filePath;

        var fileInfo = new FileInfo(Path);

        Fingerprint = new PathFingerprint(fileInfo.Length, fileInfo.LastWriteTimeUtc, DateTime.UtcNow);
        Hash = HashOf(Path);
        Size = fileInfo.Length;
        CreatedAt = fileInfo.CreationTimeUtc;
    }

    public async Task Tag(Tag tag)
    {
        await file.Tag(this, tag);
    }

    public Task Untag(Tag tag)
        => file.Untag(this, tag);

    public async Task<IEnumerable<Tag>> GetTags()
    {
        return await file.GetTags(this);
    }

    public async Task<IEnumerable<string>> GetPaths()
    {
        return await file.GetPaths(this);
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        return await file.GetNames(this);
    }

    /// <summary>
    /// Takes the stored hash of a known <paramref name="path"/> only while the stored fingerprint still matches the file on disk,
    /// otherwise the file changed (or was never fingerprinted) and is hashed again.
    /// </summary>
    public void LoadByPathFromDbWithFallback(string path)
    {
        Path = path;

        var current = new FileInfo(path);
        var known = file.LoadByPath(this);
        var trusted = known && Fingerprint.Match(none: false, some: fingerprint => fingerprint.StillMatches(current.Length, current.LastWriteTimeUtc));

        if (!trusted)
        {
            PreviousHash = known ? Option.Some(Hash) : Option<Task<string>>.None;
            LoadByPath(path);
        }
    }

    private async Task<string> HashOf(string path)
        => (await hashService.GetHash(path)).ToString();
}
