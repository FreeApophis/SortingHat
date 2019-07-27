using System.IO;

namespace SortingHat.Plugin.FileType.Detectors
{
    internal interface IFileTypeFinder
    {
        FileType? Identify(Stream stream);
        FileType? Identify(FileInfo file);
    }
}