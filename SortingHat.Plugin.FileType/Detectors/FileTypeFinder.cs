using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingHat.Plugin.FileType.Detectors
{
    [UsedImplicitly]
    public class FileTypeFinder : IFileTypeFinder
    {
        private readonly IEnumerable<IFileTypeDetector> _detectors;

        public FileTypeFinder(IEnumerable<IFileTypeDetector> detectors)
        {
            _detectors = detectors;
        }

        public FileType Identify(FileInfo file)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Identify(stream);
            }
        }

        public FileType Identify(Stream stream)
        {
            return _detectors
                .Select(detector => detector.Detect(stream))
                .FirstOrDefault(fileType => fileType != null);
        }
    }
}
