using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingHat.Plugin.FileType.Detectors
{
    class BasicFileTypeDetector : IFileTypeDetector
    {
        private const int FileTypeCategoryOffset = 1;
        private string FileTypeCategory { get; }

        private const int FileOffsetOffset = 2;
        private long FileOffset { get; }

        private const int SignatureOffset = 3;
        private string Signature { get; }

        private const int FileTypeOffset = 4;
        private string FileType { get; }

        private const int FileTypeExtensionsOffset = 5;
        private List<string> FileTypeExtensions { get; } = new List<string>();

        internal BasicFileTypeDetector(string[] parts)
        {
            FileTypeCategory = parts[FileTypeCategoryOffset];

            if (long.TryParse(parts[FileOffsetOffset], out long fileOffset))
            {
                FileOffset = fileOffset;
            }
            else
            {
                throw new Exception("FileSignatures.csv corrupt!");
            }

            Signature = parts[SignatureOffset];
            FileType = parts[FileTypeOffset];

            foreach (var extension in parts.Skip(FileTypeExtensionsOffset).Where(extension => string.IsNullOrEmpty(extension) == false))
            {
                FileTypeExtensions.Add(extension);
            }
        }

        private static bool EqualWithJoker(string lhs, string rhs)
        {
            return lhs
                .Zip(rhs, Tuple.Create)
                .Where(hex => hex.Item1 != '?' && hex.Item2 != '?')
                .All(hex => hex.Item1 == hex.Item2);
        }

        public virtual FileType Detect(Stream file)
        {
            file.Seek(FileOffset, SeekOrigin.Begin);

            var offset = 0;
            while (offset + 2 <= Signature.Length)
            {
                var octet = file.ReadByte();

                if (octet == -1)
                {
                    return null;
                }

                var signaturePart = Signature.Substring(offset, 2);
                offset += 2;

                if (EqualWithJoker(signaturePart, octet.ToString("x2")) == false)
                {
                    return null;
                }
            }

            return CreateFileType();
        }

        protected FileType CreateFileType()
        {
            return new FileType { Category = FileTypeCategory, Extensions = FileTypeExtensions, Name = FileType };
        }
    }
}
