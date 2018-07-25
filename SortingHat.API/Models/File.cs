using System.Collections.Generic;
using System.IO;

namespace SortingHat.API.Models
{
    class File
    {
        private IEnumerable<Tag> _tags;

        File(string path)
        {
        }

        File(Stream file)
        {
        }
    }
}
