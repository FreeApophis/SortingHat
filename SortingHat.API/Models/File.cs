﻿using SortingHat.API.DI;
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
        public string Path { get; set; }

        private readonly IDatabase _db;
        private readonly IHashService _hashService;

        [UsedImplicitly]
        public File(IDatabase db, IHashService hashService)
        {
            _db = db;
            _hashService = hashService;
        }


        public void DBLoadByPath()
        {
            _db.File.LoadByPath(this);
        }

        public void LoadByPath()
        {
            FileInfo fileInfo = new FileInfo(Path);

            Hash = _hashService.GetHash(Path);
            Size = fileInfo.Length;
            CreatedAt = fileInfo.CreationTimeUtc;
        }


        public async Task Tag(Tag tag)
        {
            await _db.File.Tag(this, tag);
        }

        public void Untag(Tag tag)
        {
            _db.File.Untag(this, tag);
        }

        public async Task<IEnumerable<Tag>> GetTags()
        {
            return await _db.File.GetTags(this);
        }

        public async Task<IEnumerable<string>> GetPaths()
        {
            return await _db.File.GetPaths(this);
        }

        public async Task<IEnumerable<string>> GetNames()
        {
            return await _db.File.GetNames(this);
        }
    }
}
