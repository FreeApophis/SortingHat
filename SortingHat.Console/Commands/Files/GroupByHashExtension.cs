using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands.Files
{
    internal static class GroupByHashExtension
    {
        public static IEnumerable<HashGroup> GroupByHash(this IEnumerable<API.Models.File> files)
        {
            return files.GroupBy(f => f.Hash.Result, (hash, file) => new HashGroup(hash, file.Select(f => f.Path)));
        }
    }
}