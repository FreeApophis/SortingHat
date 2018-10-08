using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;
using System.IO;

namespace SortingHat.Plugin.FileType.AutoTag
{
    class FileTypeAutoTag : ConstantAutoTag
    {
        private readonly IFileTypeFinder _fileTypeDetector;

        public FileTypeAutoTag(IFileTypeFinder fileTypeDetector)
        {
            _fileTypeDetector = fileTypeDetector;
        }

        public override string AutoTagKey => "FileType.Type";
        public override string Description => "TODO";

        protected override string HandleTag(FileInfo file)
        {
            var fileType = _fileTypeDetector.Identify(file);

            return fileType?.Name;
        }
    }
}
