using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

[UsedImplicitly]
internal class FileTypeAutoTag(IFileTypeFinder fileTypeFinder) : IAutoTag
{
    public string Name => "FileType.Type";

    public AutoTagParameter Parameter => AutoTagParameter.None;

    public string Description => "The name of the file type found from the content, such as Portable Network Graphics.";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => FileTypes.Of(file, fileTypeFinder).Select(fileType => fileType.Name);
}
