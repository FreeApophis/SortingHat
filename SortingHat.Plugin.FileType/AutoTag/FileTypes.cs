using Funcky.Monads;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

/// <summary>The type of a file, identified once however many file type variables a mask uses.</summary>
internal static class FileTypes
{
    public static Option<FileType> Of(AutoTagFile file, IFileTypeFinder fileTypeFinder)
        => file.Remember("FileType.Type", fileTypeFinder.Identify);
}
