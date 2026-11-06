using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands.Files;

internal static class GroupByHashExtension
{
    extension(IEnumerable<API.Models.File> files)
    {
        public IEnumerable<HashGroup> GroupByHash()
            => files.GroupBy(f => f.Hash.Result, (hash, file) => new HashGroup(hash, file));
    }
}
