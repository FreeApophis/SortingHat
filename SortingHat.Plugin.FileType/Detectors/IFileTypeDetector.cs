using System.IO;

namespace SortingHat.Plugin.FileType.Detectors
{
    internal interface IFileTypeDetector
    {
        FileType? Detect(Stream file);
    }
}
