using System.IO;

namespace SortingHat.Plugin.FileType
{
    public interface IFileTypeFinder
    {
        FileType Identify(Stream stream);
        FileType Identify(string path);
    }
}