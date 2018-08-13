﻿using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace SortingHat.API.Models
{
    public class File
    {
        public DateTime CreatedAt { get; }
        public long Size { get; }
        public string Hash { get; }
        public string Path { get; }

        public File(string path, string hash)
        {
            Hash = hash;

            if (System.IO.File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(Path = path);

                Size = fileInfo.Length;
                CreatedAt = fileInfo.CreationTimeUtc;
            }
            else
            {
                Path = $"File does not exists: #{path}";
            }
        }

        public File(string path, IHashService hashService)
        {
            FileInfo fileInfo = new FileInfo(Path = path);

            Hash = hashService.GetHash(path);
            Size = fileInfo.Length;
            CreatedAt = fileInfo.CreationTimeUtc;
        }

        public void Tag(IDatabase db, Tag tag)
        {
            db.File.Tag(this, tag);
        }

        public void Untag(IDatabase db, Tag tag)
        {
            db.File.Untag(this, tag);
        }

        public IEnumerable<Tag> GetTags(IDatabase db)
        {
            return Enumerable.Empty<Tag>();
        }
        public IEnumerable<string> GetPaths(IDatabase db)
        {
            return Enumerable.Empty<string>();
        }
        public IEnumerable<string> GetNames(IDatabase db)
        {
            return Enumerable.Empty<string>();
        }

    }
}
