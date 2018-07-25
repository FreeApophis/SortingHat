using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SortingHat.API.FileTypeDetection
{
    class TransportStreamDetector : BasicFileTypeDetector
    {
        private const long intervall = 188;

        internal TransportStreamDetector(string[] parts) : base(parts)
        {
        }

        public override FileType Detect(Stream file)
        {
            for (long index = 0; intervall * index < file.Length; ++index)
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
