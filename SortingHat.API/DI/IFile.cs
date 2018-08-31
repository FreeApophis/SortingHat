using SortingHat.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SortingHat.API.DI
{
    public interface IFile
    {
        bool LoadByPath(File file);
        Task Tag(File file, Tag tag);
        Task Untag(File file, Tag tag);

        Task<IEnumerable<Tag>> GetTags(File file);
        IEnumerable<string> GetPaths();
        Task<IEnumerable<string>> GetPaths(File file);
        Task<IEnumerable<string>> GetNames(File file);
        IEnumerable<File> Search(string query);
        IEnumerable<File> GetDuplicates();
    }
}
