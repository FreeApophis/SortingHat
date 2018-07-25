using System.IO;

namespace SortingHat.API.FileTypeDetection
{
    interface IFileTypeDetector
    {
        FileType Detect(Stream file);
    }
}
