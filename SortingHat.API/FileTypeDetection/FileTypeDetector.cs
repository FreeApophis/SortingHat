using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SortingHat.API.FileTypeDetection
{

    public class FileTypeDetector
    {
        private readonly List<IFileTypeDetector> _detectors = new List<IFileTypeDetector>();
        private string SignatureResource()
        {
            return "SortingHat.API.Resources.FileSignatures.csv";
        }

        private Stream SignatureStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(SignatureResource());
        }

        private void LoadDetectors()
        {
            TextReader reader = new StreamReader(SignatureStream());

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#") == false)
                {
                    _detectors.Add(FileTypeDetectorFactory.Create(line));
                }
            }
        }

        public FileTypeDetector()
        {
            LoadDetectors();
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
