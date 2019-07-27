using System.Collections.Generic;

namespace SortingHat.Plugin.FileType
{
    internal class FileType
    {
        public readonly string Category;
        public readonly string Name;
        public readonly IEnumerable<string> Extensions;

        internal FileType(string category, string name, IEnumerable<string> extensions)
        {
            Category = category;
            Name = name;
            Extensions = extensions;
        }
    }
}
