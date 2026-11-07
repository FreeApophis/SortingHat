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

    public void LoadByPathFromDb(string filePath)
    {
        Path = filePath;

        file.LoadByPath(this);
    }

    /// <summary>Hashes the file at <paramref name="filePath"/>, its fingerprint is taken right before.</summary>
    public void LoadByPath(string filePath)
    {
        Path = filePath;

        var fileInfo = new FileInfo(Path);

        Fingerprint = new PathFingerprint(fileInfo.Length, fileInfo.LastWriteTimeUtc, DateTime.UtcNow);
        Hash = hashService.GetHash(Path);
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
        var trusted = file.LoadByPath(this)
            && Fingerprint.Match(none: false, some: fingerprint => fingerprint.StillMatches(current.Length, current.LastWriteTimeUtc));

        if (!trusted)
        {
            LoadByPath(path);
        }
    }
}
