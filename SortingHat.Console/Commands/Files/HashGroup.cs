using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files
{
    internal class HashGroup
    {
        public HashGroup(string hash, IEnumerable<File> files)
        {
            Hash = hash;
            Files = files;
        }

        public string Hash { get; }
        public IEnumerable<File> Files { get; }
    }
}