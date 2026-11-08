using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.pdf'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "file-info";
    public IReadOnlyList<string> Aliases => ["info"];
    public string Description => "Shows what the database knows about the given files.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => filePathExtractor.FromFilePatterns(input.Values(Files), input.Has(FileOptions.Recursive)).Match(
            left: problems => Task.FromResult(Report(problems)),
            right: Show);

    private bool Report(IEnumerable<string> problems)
    {
        problems.ForEach(consoleWriter.WriteLine);

        return false;
    }

    private async Task<bool> Show(IReadOnlyList<string> paths)
    {
        foreach (var filePath in paths)
        {
            consoleWriter.WriteLine();
            consoleWriter.WriteLine($"File: {filePath}");

            foreach (var line in (await Report(filePath)).Lines)
            {
                consoleWriter.WriteLine(line);
            }
        }

        return true;
    }

    /// <summary>The content the database knows for the path: the content before the change when the file was edited.</summary>
    private static Option<File> StoredContent(PathState state)
        => state switch
        {
            PathState.Changed changed => changed.Stored.File,
            PathState.Known known => known.Current.File,
            _ => Option<File>.None,
        };

    private FileDetails Details(File file)
        => new(
            file.CreatedAt,
            file.Size,
            file.Hash,
            [.. fileStore.GetTags(file).Select(tag => tag.FullName)],
            fileStore.GetNames(file),
            fileStore.GetPaths(file));

    private async Task<FileInfoReport> Report(string path)
    {
        var state = await fileScanner.Examine(path);

        return FileInfoReport.Decide(StoredContent(state).Select(Details), state is PathState.Changed, Details(state.Current.File));
    }
}
