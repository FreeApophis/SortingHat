using System.Collections.Generic;

namespace SortingHat.API.FileTypeDetection
{
    public class FileType
    {
        public string Category;
        public string Name;
        public IEnumerable<string> Extensions;
    }
}
