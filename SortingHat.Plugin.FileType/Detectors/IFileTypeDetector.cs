using System.IO;

namespace SortingHat.Plugin.FileType.Detectors
{
    public interface IFileTypeDetector
    {
        FileType Detect(Stream file);
    }
}
