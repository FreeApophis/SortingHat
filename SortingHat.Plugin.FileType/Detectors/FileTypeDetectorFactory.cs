namespace SortingHat.Plugin.FileType
{
    internal static class FileTypeDetectorFactory
    {
        private const int FileTypeDetectorOffset = 0;

        internal static IFileTypeDetector Create(string signature)
        {
            var parts = signature.Split(',');

            switch (parts[FileTypeDetectorOffset])
            {
                case "basic":
                    return new BasicFileTypeDetector(parts);
                case "ts":
                    return new TransportStreamDetector(parts);
                default:
                    return null;
            }
        }
    }
}
