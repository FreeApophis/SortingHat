using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;

namespace SortingHat.API
{
    [UsedImplicitly]
    public class FilePathExtractor : IFilePathExtractor
    {
        private readonly List<string> _filePaths = new List<string>();

        public IEnumerable<string> FromFilePatterns(IEnumerable<string> filePatterns)
        {
            _filePaths.Clear();

            foreach (var filePattern in filePatterns)
            {
                FromFilePattern(filePattern);
            }

            return _filePaths;
        }

        private void FromFilePattern(string filePattern)
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

        private void AddExistingFiles(string filePath)
        {
            if (File.Exists(filePath))
            {
                _filePaths.Add(filePath);
            }
        }
    }
}
