using System.Collections.Generic;
using System.Linq;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

internal static class GroupByHashExtension
{
    extension(IEnumerable<FileAt> files)
    {
        public IEnumerable<HashGroup> GroupByHash()
            => files.GroupBy(file => file.File.Hash, (hash, group) => new HashGroup(hash, [.. group]));
    }
}
