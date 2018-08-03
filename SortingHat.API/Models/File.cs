using System;
using System.Collections.Generic;
using System.IO;
using SortingHat.API.DI;

namespace SortingHat.API.Models
{
    public class File
    {
        public DateTime CreatedAt { get; }
        public long Size { get; }
        public string Hash { get; }
        public string Path { get; }

        public File(IServices services, string path)
        {
            FileInfo fileInfo = new FileInfo(Path = path);

            Hash = services.HashService.GetHash(path);
            Size = fileInfo.Length;
            CreatedAt = fileInfo.CreationTimeUtc;
        }

        public void Tag(IServices services, Tag tag)
        {
            services.DB.TagFile(this, tag);
        }

        public void Untag(IServices services, Tag tag)
        {
            services.DB.UntagFile(this, tag);
        }

    }
}
