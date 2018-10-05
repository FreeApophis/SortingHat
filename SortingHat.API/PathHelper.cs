using System.Collections.Generic;
using System.IO;

namespace SortingHat.API
{
    public static class PathHelper
    {
        public static List<string> PathElements(DirectoryInfo directory)
        {
            var result = new List<string>();
            do
            {
                result.Add(directory.Name);
            } while ((directory = directory.Parent) != null);
            return result;
        }
    }
}
