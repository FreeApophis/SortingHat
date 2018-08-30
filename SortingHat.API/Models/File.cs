using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System;
using JetBrains.Annotations;
using System.Threading.Tasks;

namespace SortingHat.API.Models
{
    public class File
    {
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
        public Task<string> Hash { get; set; }
        public string Path { get; }

        private readonly IDatabase _db;

        public File(string path, string hash)
        {
            Hash = Task.FromResult(hash);

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

        public static IEnumerable<File> Duplicates(IDatabase db)
        {
            return db.File.GetDuplicates();
        }

        [UsedImplicitly]
        public File(IDatabase db, IHashService hashService, string path, bool loadFromDB)
        {
            Path = path;
            _db = db;

            if (loadFromDB)
            {
                Load();
            }
            else
            {
                FileInfo fileInfo = new FileInfo(Path);

                Hash = hashService.GetHash(path);
                Size = fileInfo.Length;
                CreatedAt = fileInfo.CreationTimeUtc;
            }
        }

        private void Load()
        {
            _db.File.Load(this);
        }


        public void Tag(Tag tag)
        {
            _db.File.Tag(this, tag);
        }

        public void Untag(Tag tag)
        {
            _db.File.Untag(this, tag);
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
