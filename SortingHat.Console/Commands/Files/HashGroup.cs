using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

internal class HashGroup(string hash, IEnumerable<File> files)
{
    public string Hash { get; } = hash;
    public IEnumerable<File> Files { get; } = files;
}