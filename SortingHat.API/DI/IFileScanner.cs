using System.Threading.Tasks;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

/// <summary>Reads files from disk.</summary>
public interface IFileScanner
{
    /// <summary>Hashes the file at <paramref name="path"/>; its fingerprint is taken right before.</summary>
    Task<FileAt> Scan(string path);

    /// <summary>Tells what hat knows about <paramref name="path"/>. The file is hashed only when the stored fingerprint no longer matches it.</summary>
    Task<PathState> Examine(string path);
}
