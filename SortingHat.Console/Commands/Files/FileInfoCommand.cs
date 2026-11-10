using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.App.Files;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, FileReports fileReports, IAutoTagHandler autoTagHandler) : IDeclaredCommand
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

            foreach (var line in (await fileReports.Report(filePath)).Lines)
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
}
