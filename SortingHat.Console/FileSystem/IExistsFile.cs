using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.CLI.FileSystem
{
    interface IExistsFile
    {
        bool Exists(string path);
    }
}
