using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType;

[UsedImplicitly]
internal class IdentifyCommand(IConsoleWriter consoleWriter, IFileTypeFinder fileTypeFinder, IFilePathExtractor filePathExtractor) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.pdf'.", Arity = Arity.OneOrMore };
    private static readonly OptionDefinition Recursive = new() { Name = "recursive", ShortName = "r", Description = "Search the subfolders too; a folder argument needs it" };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "identify";
    public IReadOnlyList<string> Aliases => ["id"];
    public string Description => "Identifies the real type of files from their content, whatever their extension.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];
    public IReadOnlyList<OptionDefinition> Options => [Recursive];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(filePathExtractor.FromFilePatterns(input.Values(Files), input.Has(Recursive)).Match(left: Report, right: Identify));

    private bool Report(IEnumerable<string> problems)
    {
        problems.ForEach(consoleWriter.WriteLine);

        return false;
    }

    private bool Identify(IReadOnlyList<string> paths)
    {
        foreach (var path in paths)
        {
            consoleWriter.WriteLine($"File: {path}");

            foreach (var line in Lines(fileTypeFinder.Identify(new System.IO.FileInfo(path))))
            {
                consoleWriter.WriteLine(line);
            }
        }

        return true;
    }

    private static IReadOnlyList<string> Lines(Option<FileType> fileType)
        => fileType.Match<IReadOnlyList<string>>(
            none: ["Unknown filetype"],
            some: type => [$"Category : {type.Category}", $"Name     : {type.Name}", $"Extension: {string.Join(",", type.Extensions)}"]);
}
