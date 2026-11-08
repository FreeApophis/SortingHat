using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore, IAutoTagHandler autoTagHandler) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.pdf'.", Arity = Arity.OneOrMore };

    private static readonly OptionDefinition Variables = new() { Name = "variables", ShortName = "v", Description = "Also shows what every tag variable gives for the file, such as {CreatedAt.Year} = 2018, for writing tag masks." };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "file-info";
    public IReadOnlyList<string> Aliases => ["info"];
    public string Description => "Shows what the database knows about the given files.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive, Variables];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => filePathExtractor.FromFilePatterns(input.Values(Files), input.Has(FileOptions.Recursive)).Match(
            left: problems => Task.FromResult(Report(problems)),
            right: paths => Show(paths, input.Has(Variables)));

    private bool Report(IEnumerable<string> problems)
    {
        problems.ForEach(consoleWriter.WriteLine);

        return false;
    }

    private async Task<bool> Show(IReadOnlyList<string> paths, bool variables)
    {
        foreach (var filePath in paths)
        {
            consoleWriter.WriteLine();
            consoleWriter.WriteLine($"File: {filePath}");

            foreach (var line in (await Report(filePath)).Lines)
            {
                consoleWriter.WriteLine(line);
            }

            if (variables)
            {
                ShowTagVariables(filePath);
            }
        }

        return true;
    }

    /// <summary>What every tag variable would put into a tag for the file, indexed or not.</summary>
    private void ShowTagVariables(string path)
    {
        IReadOnlyList<string> lines = [.. TagVariables.Lines(autoTagHandler, new AutoTagFile(new System.IO.FileInfo(path)))];

        if (lines.Count > 0)
        {
            consoleWriter.WriteLine("Tag variables:");
            lines.ForEach(consoleWriter.WriteLine);
        }
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
