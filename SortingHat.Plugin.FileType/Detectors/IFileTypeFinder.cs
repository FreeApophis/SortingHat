using System.IO;
using Funcky.Monads;

namespace SortingHat.Plugin.FileType.Detectors;

internal interface IFileTypeFinder
{
    /// <summary>The type of the <paramref name="file"/> from its first bytes, none when no signature matches.</summary>
    Option<FileType> Identify(FileInfo file);
}
