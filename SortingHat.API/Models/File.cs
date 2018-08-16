using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System;

namespace SortingHat.API.Models
{
    public class File
    {
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public string Path { get; }

        private readonly IDatabase _db;

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


        public File(IDatabase db, string path)
        {
            Path = path;
            _db = db;

            Load();
        }

        private void Load()
        {
            _db.File.Load(this);
        }


        public void Tag(IDatabase db, Tag tag)
        {
            db.File.Tag(this, tag);
        }

        public void Untag(IDatabase db, Tag tag)
        {
            db.File.Untag(this, tag);
        }

        public IEnumerable<Tag> GetTags()
        {
            return _db.File.GetTags(this);
        }
        public IEnumerable<string> GetPaths()
        {
            return _db.File.GetPaths(this);
        }
        public IEnumerable<string> GetNames()
        {
            return _db.File.GetNames(this);
        }

    }
}
