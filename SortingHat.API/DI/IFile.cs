using SortingHat.API.Models;
using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IFile
    {
        bool LoadByPath(File file);
        void Tag(File file, Tag tag);
        void Untag(File file, Tag tag);

        IEnumerable<Tag> GetTags(File file);
        IEnumerable<string> GetPaths();
        IEnumerable<string> GetPaths(File file);
        IEnumerable<string> GetNames(File file);
        IEnumerable<File> Search(string query);
        IEnumerable<File> GetDuplicates();
    }
}
