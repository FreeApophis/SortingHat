using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingHat.API
{
    [UsedImplicitly]
    public class FilePathExtractor : IFilePathExtractor
    {
        public IEnumerable<string> FromFilePatterns(IEnumerable<string> filePatterns, bool recursive)
        {
            return filePatterns.Aggregate(new List<string>(), (filePaths, filePattern) => FromFilePattern(filePaths, filePattern, recursive, Directory.GetCurrentDirectory()));
        }

        private List<string> FromFilePattern(List<string> filePaths, string filePattern, bool recursive, string baseDirectory)
        {
            if (filePattern.Contains("*") || filePattern.Contains("?"))
            {
                foreach (var filePath in Directory.GetFiles(baseDirectory, filePattern))
                {
                    AddExistingFiles(filePaths, filePath);
                }

                if (recursive)
                {
                    foreach (var directory in Directory.GetDirectories(baseDirectory))
                    {
                        FromFilePattern(filePaths, filePattern, true, directory);
                    }
                }
            }
            else
            {
                var absolutePath = Path.Combine(baseDirectory, filePattern);

                AddExistingFiles(filePaths, absolutePath);
            }

            return filePaths;
        }

        private void AddExistingFiles(List<string> filePaths, string filePath)
        {
            var normalizedFilePath = Path.GetFullPath(filePath);

            if (File.Exists(normalizedFilePath))
            {
                filePaths.Add(normalizedFilePath);
            }
        }
    }
}
