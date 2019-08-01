using JetBrains.Annotations;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SortingHat.API.Models
{
    public class File
    {
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
        public Task<string> Hash { get; set; } = Task.FromResult("BADHASH");
        public string Path { get; set; } = "BADPATH";

        private readonly IFile _file;
        private readonly IHashService _hashService;

        [UsedImplicitly]
        public File(IFile file, IHashService hashService)
        {
            _file = file;
            _hashService = hashService;
        }


        public void LoadByPathFromDb(string filePath)
        {
            Path = filePath;

            _file.LoadByPath(this);
        }

        public void LoadByPath(string filePath)
        {
            Path = filePath;

            if (Path is { })
            {
                var fileInfo = new FileInfo(Path);

                Hash = _hashService.GetHash(Path);
                Size = fileInfo.Length;
                CreatedAt = fileInfo.CreationTimeUtc;
            }
        }


        public async Task Tag(Tag tag)
        {
            await _file.Tag(this, tag);
        }

        public void Untag(Tag tag)
        {
            _file.Untag(this, tag);
        }

        public async Task<IEnumerable<Tag>> GetTags()
        {
            return await _file.GetTags(this);
        }

        public async Task<IEnumerable<string>> GetPaths()
        {
            return await _file.GetPaths(this);
        }

        public async Task<IEnumerable<string>> GetNames()
        {
            return await _file.GetNames(this);
        }

        public void LoadByPathFromDbWithFallback(string path)
        {
            Path = path;

            if (_file.LoadByPath(this)) { return; }

            if (Path is { })
            {
                var fileInfo = new FileInfo(Path);

                Hash = _hashService.GetHash(Path);
                Size = fileInfo.Length;
                CreatedAt = fileInfo.CreationTimeUtc;
            }
        }
    }
}
