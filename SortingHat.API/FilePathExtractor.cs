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
            return filePatterns.SelectMany(pattern => PathFromFilePattern(pattern, recursive, Directory.GetCurrentDirectory()));
        }

        private IEnumerable<string> PathFromFilePattern( string filePattern, bool recursive, string baseDirectory)
        {
            if (filePattern.Contains("*") || filePattern.Contains("?"))
            {
                foreach (var filePath in Directory.GetFiles(baseDirectory, filePattern))
                {
                    if (NormalizedExistingPath(filePath) is { } path)
                    {
                        yield return path;
                    }
                }

                if (recursive)
                {
                    foreach (var directory in Directory.GetDirectories(baseDirectory))
                    {
                        foreach(var path in PathFromFilePattern(filePattern, true, directory))
                        {
                            yield return path;
                        }
                    }
                }
            }
            else
            {
                var fullPath = Path.Combine(baseDirectory, filePattern);

                if (NormalizedExistingPath(fullPath) is { } path)
                {
                    yield return path;
                }
            }
        }

        private string? NormalizedExistingPath(string filePath)
        {
            var normalizedFilePath = Path.GetFullPath(filePath);

            return File.Exists(normalizedFilePath)
                ? normalizedFilePath
                : null;
        }
    }
}
