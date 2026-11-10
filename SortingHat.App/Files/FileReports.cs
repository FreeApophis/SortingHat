using System.Linq;
using System.Threading.Tasks;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.App.Files;

/// <summary>What the database knows about a file, for file-info and the details of a search result.</summary>
[UsedImplicitly]
public sealed class FileReports(IFileScanner fileScanner, IFile fileStore)
{
    /// <summary>Examines the content now at the <paramref name="path"/> and decides what to tell about it.</summary>
    public async Task<FileInfoReport> Report(string path)
    {
        var state = await fileScanner.Examine(path);

        return FileInfoReport.Decide(StoredContent(state).Select(Details), state is PathState.Changed, Details(state.Current.File));
    }

    /// <summary>What is stored for the <paramref name="file"/>: its tags in tree order, its names and its paths.</summary>
    public FileDetails Details(File file)
        => new(
            file.CreatedAt,
            file.Size,
            file.Hash,
            [.. fileStore.GetTags(file).Select(tag => tag.FullName)],
            fileStore.GetNames(file),
            fileStore.GetPaths(file));

    /// <summary>The content the database knows for the path: the content before the change when the file was edited.</summary>
    private static Option<File> StoredContent(PathState state)
        => state switch
        {
            PathState.Changed changed => changed.Stored.File,
            PathState.Known known => known.Current.File,
            _ => Option<File>.None,
        };
}
