using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace SortingHat.Plugin.FileType.Detectors;

[UsedImplicitly]
internal class FileTypeFinder(IEnumerable<IFileTypeDetector> detectors) : IFileTypeFinder
{
    public FileType? Identify(FileInfo file)
    {
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        return Identify(stream);
    }

    public FileType? Identify(Stream stream)
    {
        return detectors
            .Select(detector => detector.Detect(stream))
            .FirstOrDefault(fileType => fileType != null);
    }
}
