using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

/// <summary>The files hat knows: their content, the paths where it lives, its names and its tags.</summary>
public interface IFile
{
    /// <summary>What is stored for <paramref name="path"/>, with its fingerprint.</summary>
    Option<FileAt> Find(string path);

    /// <summary>Stores the <paramref name="file"/> like <see cref="Store"/> and tags its content.</summary>
    void Tag(FileAt file, Tag tag);

    void Untag(File file, Tag tag);

    /// <summary>Stores the <paramref name="file"/>: its content when it is new, its name, and its path with the fingerprint.</summary>
    void Store(FileAt file);

    IReadOnlyList<Tag> GetTags(File file);

    /// <summary>Every path hat knows.</summary>
    IReadOnlyList<string> GetPaths();

    /// <summary>The paths where the <paramref name="file"/> lives.</summary>
    IReadOnlyList<string> GetPaths(File file);

    IReadOnlyList<string> GetNames(File file);

    /// <summary>
    /// Records that the <paramref name="file"/>, found at its <see cref="FileAt.Path"/>, now lives at <paramref name="destinationPath"/>.
    /// Only this file's path changes, its other paths stay.
    /// </summary>
    void MovePath(FileAt file, string destinationPath);

    /// <summary>
    /// Forgets the <paramref name="paths"/>, for example because the files no longer exist there. The files themselves keep
    /// their tags and names: storing or tagging the same content at a new place links it to them again.
    /// </summary>
    void RemovePaths(IEnumerable<string> paths);

    SearchResult Search(string query);

    /// <summary>The files stored at more than one path.</summary>
    IReadOnlyList<File> GetDuplicates();
}
