using System;
using System.Collections.Generic;

namespace SortingHat.API;

internal static class PathHelper
{
    private static readonly char[] Separators = ['\\', '/'];

    /// <summary>
    /// The folders of the <paramref name="folder"/> path from its root, each one a valid tag level: a drive is only its letter,
    /// so <c>C:\User\Path</c> gives C, User, Path. A UNC path gives its server and share, a Unix root has no name.
    /// </summary>
    public static IReadOnlyList<string> Folders(string folder)
        => [.. Array.ConvertAll(folder.Split(Separators, StringSplitOptions.RemoveEmptyEntries), part => part.TrimEnd(':'))];
}
