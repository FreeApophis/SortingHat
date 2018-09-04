using System.Collections.Generic;
using System.IO;

namespace SortingHat.API
{
    public class FilePathExtractor
    {
        private readonly List<string> _filePaths = new List<string>();
        public IEnumerable<string> FilePaths => _filePaths;

        public FilePathExtractor(IEnumerable<string> filePatterns)
        {
            GetFilePaths(filePatterns);
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
