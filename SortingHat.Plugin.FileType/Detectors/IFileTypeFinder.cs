using System.IO;

namespace SortingHat.Plugin.FileType.Detectors
{
    public interface IFileTypeFinder
    {
        FileType Identify(Stream stream);
        FileType Identify(string path);
    }
}