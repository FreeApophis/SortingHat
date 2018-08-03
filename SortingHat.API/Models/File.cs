using System;
using System.Collections.Generic;
using System.IO;
using SortingHat.API.DI;

namespace SortingHat.API.Models
{
    public class File
    {
        private IEnumerable<Tag> _tags;

        private readonly string _hash;
        public string Hash => _hash;

        public File(string path)
        {
            _hash = "null";
        }

        public File(Stream file)
        {
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
