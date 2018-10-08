using System.Collections.Generic;
using System.IO;

namespace SortingHat.API
{
    public static class PathHelper
    {
        public static List<string> PathElements(DirectoryInfo directory)
        {
            var result = new List<string> { directory.Name };

            while ((directory = directory.Parent) != null)
            {
                result.Add(directory.Name);
            }

            return result;
        }
    }
}
