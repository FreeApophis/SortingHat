namespace SortingHat.API.FileTypeDetection
{
    class FileTypeDetectorFactory
    {
        const int FileTypeDetectorOffset = 0;

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
