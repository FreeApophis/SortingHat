using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType;

[UsedImplicitly]
internal class IdentifyCommand(IConsoleWriter consoleWriter, IFileTypeFinder fileTypeFinder, IFilePathExtractor filePathExtractor) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.pdf'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "identify";
    public IReadOnlyList<string> Aliases => ["id"];
    public string Description => "This command identifies the real file type (ignoring the file-extension)";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        foreach (var path in filePathExtractor.FromFilePatterns(input.Values(Files), recursive: false))
        {
            consoleWriter.WriteLine($"File: {path}");

            foreach (var line in Lines(fileTypeFinder.Identify(new System.IO.FileInfo(path))))
            {
                consoleWriter.WriteLine(line);
            }
        }

        return Task.FromResult(true);
    }

    private static IReadOnlyList<string> Lines(Option<FileType> fileType)
        => fileType.Match<IReadOnlyList<string>>(
            none: ["Unknown filetype"],
            some: type => [$"Category : {type.Category}", $"Name     : {type.Name}", $"Extension: {string.Join(",", type.Extensions)}"]);
}
