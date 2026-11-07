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

    /// <summary>
    /// Forgets the <paramref name="paths"/>, for example because the files no longer exist there. The files themselves keep
    /// their tags and names: storing or tagging the same content at a new place links it to them again.
    /// </summary>
    void RemovePaths(IEnumerable<string> paths);

    SearchResult Search(string query);

    IEnumerable<File> GetDuplicates();
}
