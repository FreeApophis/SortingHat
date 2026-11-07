using System.Collections.Generic;
using System.IO;

namespace SortingHat.API;

internal static class PathHelper
{
    public static List<string> PathElements(DirectoryInfo? directory)
    {
        var result = new List<string>();

        for (; directory != null; directory = directory.Parent)
        {
            result.Add(directory.Name);
        }

        return result;
    }
}
