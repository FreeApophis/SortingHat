using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands.Files;

/// <summary>What the database knows about one content.</summary>
internal sealed record FileDetails(DateTime CreatedAt, long Size, string Hash, IReadOnlyList<string> Tags, IReadOnlyList<string> Names, IReadOnlyList<string> Paths)
{
    public IReadOnlyList<string> Lines
        =>
        [
            $"CreatedAt (oldest): {CreatedAt}",
            $"File Size: {Size}",
            $"File Hash: {Hash}",
            .. Tags.Select(tag => $"Tag: {tag}"),
            .. Names.Select(name => $"Name: {name}"),
            .. Paths.Select(path => $"Path: {path}"),
        ];
}
