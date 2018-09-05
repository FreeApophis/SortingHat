using System;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.Tagging;

namespace SortingHat.Plugin.FileType
{
    internal class FileTypeAutoTag : IAutoTag
    {
        const string FileTypeCategory = "FileType.Category";
        const string FileTypeType = "FileType.Type";
        const string FileTypeExtension = "FileType.Extension";

        public IEnumerable<string> PossibleAutoTags => new List<string>() { FileTypeCategory, FileTypeType, FileTypeExtension };

        private IFileTypeFinder _fileTypeDetector;

        public FileTypeAutoTag(IFileTypeFinder fileTypeDetector)
        {
            _fileTypeDetector = fileTypeDetector;
        }

        public string HandleTag(string autoTag, string fileName)
        {
            var fileType = _fileTypeDetector.Identify(fileName);

            switch (autoTag)
            {
                case FileTypeCategory:
                    return fileType.Category;
                case FileTypeType:
                    return fileType.Category;
                case FileTypeExtension:
                    return fileType.Extensions.FirstOrDefault();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
