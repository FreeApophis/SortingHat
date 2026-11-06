using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    public void LoadByPathFromDb(string filePath)
    {
        Path = filePath;

        file.LoadByPath(this);
    }

    public void LoadByPath(string filePath)
    {
        Path = filePath;

        if (Path is { })
        {
            var fileInfo = new FileInfo(Path);

            Hash = hashService.GetHash(Path);
            Size = fileInfo.Length;
            CreatedAt = fileInfo.CreationTimeUtc;
        }
    }

    public async Task Tag(Tag tag)
    {
        await file.Tag(this, tag);
    }

    public void Untag(Tag tag)
    {
        file.Untag(this, tag);
    }

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

    public void LoadByPathFromDbWithFallback(string path)
    {
        Path = path;

        if (file.LoadByPath(this))
        {
            return;
        }

        if (Path is { })
        {
            var fileInfo = new FileInfo(Path);

            Hash = hashService.GetHash(Path);
            Size = fileInfo.Length;
            CreatedAt = fileInfo.CreationTimeUtc;
        }
    }
}
