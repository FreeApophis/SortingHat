using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingHat.Plugin.FileType
{

    public class FileTypeFinder : IFileTypeFinder
    {
        private readonly IEnumerable<IFileTypeDetector> _detectors;

        FileTypeFinder(IEnumerable<IFileTypeDetector> detectors)
        {
            _detectors = detectors;
        }

        public FileType Identify(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
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
