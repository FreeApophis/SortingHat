using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, Func<File> newFile) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.pdf'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "file-info";
    public IReadOnlyList<string> Aliases => ["info"];
    public string Description => "Shows all available information about the current file.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public async Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        foreach (var filePath in filePathExtractor.FromFilePatterns(input.Values(Files), input.Has(FileOptions.Recursive)))
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

    private static async Task<FileDetails> Details(File file)
        => new(
            file.CreatedAt,
            file.Size,
            await file.Hash,
            [.. (await file.GetTags()).Select(tag => tag.FullName)],
            [.. await file.GetNames()],
            [.. await file.GetPaths()]);

    private async Task<FileInfoReport> Report(string path)
    {
        var stored = newFile();
        var known = stored.LoadByPathFromDb(path);

        // the stored hash while the file is unchanged, otherwise the file is hashed again
        var current = newFile();
        current.LoadByPathFromDbWithFallback(path);

        return FileInfoReport.Decide(
            known ? await Details(stored) : Option<FileDetails>.None,
            await current.HasChanged(),
            await Details(current));
    }
}
