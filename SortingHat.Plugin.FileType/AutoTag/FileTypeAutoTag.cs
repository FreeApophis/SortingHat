using System.IO;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

internal class FileTypeAutoTag(IFileTypeFinder fileTypeDetector) : ConstantAutoTag
{
    public override string AutoTagKey => "FileType.Type";
    public override string Description => "TODO";

    protected override string? HandleTag(FileInfo file)
    {
        var fileType = fileTypeDetector.Identify(file);

        return fileType?.Name;
    }
}
