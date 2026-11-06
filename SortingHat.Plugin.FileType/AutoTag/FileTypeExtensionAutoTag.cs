using System.IO;
using System.Linq;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

internal class FileTypeExtensionAutoTag(IFileTypeFinder fileTypeDetector) : ConstantAutoTag
{
    public override string AutoTagKey => "FileType.Extension";
    public override string Description => "TODO";

    protected override string? HandleTag(FileInfo file)
    {
        var fileType = fileTypeDetector.Identify(file);

        return fileType?.Extensions?.FirstOrDefault();
    }
}
