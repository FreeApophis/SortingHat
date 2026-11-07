using System.Collections.Generic;
using System.Threading.Tasks;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

public interface IFile
{
    bool LoadByPath(File file);

    Task Tag(File file, Tag tag);

    Task Untag(File file, Tag tag);

    bool Store(File file);

    Task<IEnumerable<Tag>> GetTags(File file);

    IEnumerable<string> GetPaths();

    Task<IEnumerable<string>> GetPaths(File file);

    Task<IEnumerable<string>> GetNames(File file);

    /// <summary>
    /// Records that the <paramref name="file"/>, found at its <see cref="File.Path"/>, now lives at <paramref name="destinationPath"/>.
    /// Only this file's path changes, its other paths stay.
    /// </summary>
    Task MovePath(File file, string destinationPath);

    SearchResult Search(string query);

    IEnumerable<File> GetDuplicates();
}
