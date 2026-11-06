using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
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

            var file = newFile();

            file.LoadByPathFromDb(filePath);
            if (file.Hash == null)
            {
                consoleWriter.WriteLine("File not in index!");
            }
            else
            {
                consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                consoleWriter.WriteLine($"File Size: {file.Size}");
                consoleWriter.WriteLine($"File Hash: {await file.Hash}");

                foreach (var tag in await file.GetTags())
                {
                    consoleWriter.WriteLine($"Tag: {tag.FullName}");
                }

                foreach (var name in await file.GetNames())
                {
                    consoleWriter.WriteLine($"Name: {name}");
                }

                foreach (var path in await file.GetPaths())
                {
                    consoleWriter.WriteLine($"Path: {path}");
                }
            }
        }

        return true;
    }
}
