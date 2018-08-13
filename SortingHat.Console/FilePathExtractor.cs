using System.Collections.Generic;
using System.IO;

namespace SortingHat.CLI
{
    class FilePathExtractor
    {
        private List<string> _filePaths = new List<string>();

        public IEnumerable<string> FilePaths => _filePaths;

        public FilePathExtractor(IEnumerable<string> filePatterns)
        {

        }

        public FilePathExtractor(string filePattern)
        {

        }

        private void GetFilePaths(IEnumerable<string> filePatterns)
        {
            foreach (var filePattern in filePatterns)
            {
                if (filePattern.Contains("*") || filePattern.Contains("?"))
                {
                    foreach (var filePath in Directory.GetFiles(Directory.GetCurrentDirectory(), filePattern))
                    {
                        AddExistingFiles(filePath);
                    }
                }

                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filePattern);
                AddExistingFiles(absolutePath);
            }
        }

        private void AddExistingFiles(string filePath)
        {
            if (File.Exists(filePath))
            {
                _filePaths.Add(filePath);
            }
        }
    }
}
