using System.IO;

namespace SortingHat.API.FileTypeDetection
{
    internal interface IFileTypeDetector
    {
        FileType Detect(Stream file);
    }
}
