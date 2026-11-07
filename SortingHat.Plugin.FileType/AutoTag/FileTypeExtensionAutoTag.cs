using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

[UsedImplicitly]
internal class FileTypeExtensionAutoTag(IFileTypeFinder fileTypeFinder) : IAutoTag
{
    public string Name => "FileType.Extension";

    public AutoTagParameter Parameter => AutoTagParameter.None;

    public string Description => "The usual extension of the file type found from the content, such as png.";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => FileTypes.Of(file, fileTypeFinder).SelectMany(fileType => fileType.Extensions.FirstOrNone());
}
