using System.IO;

namespace SortingHat.Plugin.FileType
{
    internal interface IFileTypeDetector
    {
        FileType Detect(Stream file);
    }
}
