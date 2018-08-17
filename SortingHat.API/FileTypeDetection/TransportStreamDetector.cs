using System.IO;

namespace SortingHat.API.FileTypeDetection
{
    class TransportStreamDetector : BasicFileTypeDetector
    {
        private const long Intervall = 188;

        internal TransportStreamDetector(string[] parts) : base(parts)
        {
        }

        public override FileType Detect(Stream file)
        {
            for (long index = 0; Intervall * index < file.Length; ++index)
            {
                file.Seek(0, SeekOrigin.Begin);
                if (file.ReadByte() != 0x47)
                {
                    return null;
                }
            }

            return CreateFileType();
        }
    }
}
