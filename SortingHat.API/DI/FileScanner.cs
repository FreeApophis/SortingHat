using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SortingHat.API.Models;
using File = SortingHat.API.Models.File;

namespace SortingHat.API.DI;

[UsedImplicitly]
internal sealed class FileScanner(IHashService hashService, IFile fileStore) : IFileScanner
{
    public async Task<FileAt> Scan(string path)
    {
        var fileInfo = new FileInfo(path);
        var fingerprint = new PathFingerprint(fileInfo.Length, fileInfo.LastWriteTimeUtc, DateTime.UtcNow);

        return new FileAt(new File(await hashService.GetHash(path), fileInfo.Length, fileInfo.CreationTimeUtc), path, fingerprint);
    }

    public Task<PathState> Examine(string path)
    {
        var stored = fileStore.Find(path);
        var onDisk = new FileInfo(path);

        return PathState.Trust(stored, onDisk.Length, onDisk.LastWriteTimeUtc)
            .Match(none: async () => PathState.Decide(stored, await Scan(path)), some: Task.FromResult);
    }
}
