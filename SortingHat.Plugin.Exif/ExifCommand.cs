using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API.DI;
using static MetadataExtractor.ImageMetadataReader;

namespace SortingHat.Plugin.Exif;

[UsedImplicitly]
internal class ExifCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.jpg'.", Arity = Arity.OneOrMore };
    private static readonly OptionDefinition Recursive = new() { Name = "recursive", ShortName = "r", Description = "Search the subfolders too; a folder argument needs it" };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "exif";
    public string Description => "Reads exif information from files.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];
    public IReadOnlyList<OptionDefinition> Options => [Recursive];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(filePathExtractor.FromFilePatterns(input.Values(Files), input.Has(Recursive)).Match(left: Report, right: Show));

    private bool Report(IEnumerable<string> problems)
    {
        problems.ForEach(consoleWriter.WriteLine);

        return false;
    }

    private bool Show(IReadOnlyList<string> paths)
    {
        paths
            .SelectMany(path => ReadMetadata(path))
            .SelectMany(directory => directory.Tags.Select(tag => $"{directory.Name} - {tag.Name} ({tag.Type}) = {tag.Description}"))
            .ForEach(consoleWriter.WriteLine);

        return true;
    }
}
