using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.Models
{
    class File
    {
        private string _path;
        private IEnumerable<Tag> _tags;

        File(string path)
        {
            _path = path;
        }

    }
}
