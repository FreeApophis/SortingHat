using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

[UsedImplicitly]
internal class FileTypeCategoryAutoTag(IFileTypeFinder fileTypeFinder) : IAutoTag
{
    public string Name => "FileType.Category";

    public AutoTagParameter Parameter => AutoTagParameter.None;

    public string Description => "The category of the file type found from the content, such as image or document.";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => FileTypes.Of(file, fileTypeFinder).Select(fileType => fileType.Category);
}
