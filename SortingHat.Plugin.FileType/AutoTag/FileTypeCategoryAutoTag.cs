using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;
using System.IO;

namespace SortingHat.Plugin.FileType.AutoTag
{
    [UsedImplicitly]
    internal class FileTypeCategoryAutoTag : ConstantAutoTag
    {
        public override string AutoTagKey => "FileType.Category";
        public override string Description => "TODO";

        private readonly IFileTypeFinder _fileTypeDetector;

        public FileTypeCategoryAutoTag(IFileTypeFinder fileTypeDetector)
        {
            _fileTypeDetector = fileTypeDetector;
        }

        protected override string HandleTag(FileInfo file)
        {
            var fileType = _fileTypeDetector.Identify(file);

            return fileType?.Category;
        }
    }
}
