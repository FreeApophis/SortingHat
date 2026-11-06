using System.IO;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

[UsedImplicitly]
internal class FileTypeCategoryAutoTag(IFileTypeFinder fileTypeDetector) : ConstantAutoTag
{
    public override string AutoTagKey => "FileType.Category";
    public override string Description => "TODO";

    protected override string? HandleTag(FileInfo file)
    {
        var fileType = fileTypeDetector.Identify(file);

        return fileType?.Category;
    }
}
