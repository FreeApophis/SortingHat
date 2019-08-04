using System.Collections.Generic;

namespace SortingHat.CLI.Commands.Files
{
    internal class HashGroup
    {
        public HashGroup(string hash, IEnumerable<string> paths)
        {
            Hash = hash;
            Paths = paths;
        }
        private string Hash { get; }
        public IEnumerable<string> Paths { get; }
    }
}