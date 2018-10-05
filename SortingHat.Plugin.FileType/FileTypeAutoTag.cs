using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.FileType.Detectors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.FileType
{
    [UsedImplicitly]
    internal class FileTypeAutoTag : IAutoTag
    {
        private const string FileTypeCategory = "FileType.Category";
        private const string FileTypeType = "FileType.Type";
        private const string FileTypeExtension = "FileType.Extension";

        public IEnumerable<string> PossibleAutoTags => new List<string> { FileTypeCategory, FileTypeType, FileTypeExtension };

        private readonly IFileTypeFinder _fileTypeDetector;

        public FileTypeAutoTag(IFileTypeFinder fileTypeDetector)
        {
            _fileTypeDetector = fileTypeDetector;
        }

        public string HandleTag(string autoTag, string fileName)
        {
            var fileType = _fileTypeDetector.Identify(fileName);

            if (fileType == null)
            {
                return null;
            }

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
