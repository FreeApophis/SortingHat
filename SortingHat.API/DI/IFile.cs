using SortingHat.API.Models;
using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IFile
    {
        void Tag(File file, Tag tag);
        void Untag(File file, Tag tag);

        IEnumerable<File> Search(string query);
    }
}
