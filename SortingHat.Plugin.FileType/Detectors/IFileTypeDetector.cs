using System.IO;

namespace SortingHat.Plugin.FileType
{
    public interface IFileTypeDetector
    {
        FileType Detect(Stream file);
    }
}
