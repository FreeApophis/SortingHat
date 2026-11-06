using System.IO;
using System.Linq;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType.AutoTag;

internal class FileTypeExtensionAutoTag : ConstantAutoTag
{
    private readonly IFileTypeFinder _fileTypeDetector;

    public FileTypeExtensionAutoTag(IFileTypeFinder fileTypeDetector)
    {
        _fileTypeDetector = fileTypeDetector;
    }

    public override string AutoTagKey => "FileType.Extension";
    public override string Description => "TODO";

    protected override string? HandleTag(FileInfo file)
    {
        var fileType = _fileTypeDetector.Identify(file);

        return fileType?.Extensions?.FirstOrDefault();
    }
}
