using System.IO;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.Plugin.FileType.Detectors;

[UsedImplicitly]
internal class FileTypeFinder(Signatures signatures) : IFileTypeFinder
{
    public Option<FileType> Identify(FileInfo file)
    {
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var head = new byte[signatures.HeadLength];
        var length = stream.ReadAtLeast(head, head.Length, throwOnEndOfStream: false);

        return signatures.Identify(head[..length]);
    }
}
