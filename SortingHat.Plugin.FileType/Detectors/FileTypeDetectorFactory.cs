using System;

namespace SortingHat.Plugin.FileType.Detectors
{
    internal static class FileTypeDetectorFactory
    {
        private const int FileTypeDetectorOffset = 0;

        internal static IFileTypeDetector Create(string signature)
        {
            var parts = signature.Split(',');

            return parts[FileTypeDetectorOffset] switch
            {
                "basic" => new BasicFileTypeDetector(parts),
                "ts" => new TransportStreamDetector(parts),
                _ => throw new NotImplementedException("Unknown FileTypeDetector")
            };
        }
    }
}
