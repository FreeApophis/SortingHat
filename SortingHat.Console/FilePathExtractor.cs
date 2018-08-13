using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingHat.CLI
{
    class FilePathExtractor
    {
        private List<string> _filePaths = new List<string>();
        public IEnumerable<string> FilePaths => _filePaths;

        public FilePathExtractor(IEnumerable<string> filePatterns)
        {
            GetFilePaths(filePatterns);
        }

        public FilePathExtractor(string filePattern)
        {
            GetFilePaths(Enumerable.Repeat(filePattern, 1));
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
                else
                {
                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filePattern);
                    AddExistingFiles(absolutePath);
                }
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
