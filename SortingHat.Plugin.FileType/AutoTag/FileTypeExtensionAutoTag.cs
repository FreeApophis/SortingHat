using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;
using System.IO;
using System.Linq;

namespace SortingHat.Plugin.FileType.AutoTag
{
    class FileTypeExtensionAutoTag : ConstantAutoTag
    {
        private readonly IFileTypeFinder _fileTypeDetector;

        public FileTypeExtensionAutoTag(IFileTypeFinder fileTypeDetector)
        {
            _fileTypeDetector = fileTypeDetector;
        }
        public override string AutoTagKey => "FileType.Extension";
        public override string Description => "TODO";

        protected override string HandleTag(FileInfo file)
        {
            var fileType = _fileTypeDetector.Identify(file);

            return fileType?.Extensions?.FirstOrDefault();
        }
    }
}
