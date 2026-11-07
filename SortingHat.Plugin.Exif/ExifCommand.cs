using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using static MetadataExtractor.ImageMetadataReader;

namespace SortingHat.Plugin.Exif;

[UsedImplicitly]
internal class ExifCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Files = new() { Name = "file", Description = "Files or file patterns such as '*.jpg'.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "exif";
    public string Description => "Reads exif information from files.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Files];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var lines = filePathExtractor
            .FromFilePatterns(input.Values(Files), recursive: false)
            .SelectMany(ReadMetadata)
            .SelectMany(directory => directory.Tags.Select(tag => $"{directory.Name} - {tag.Name} ({tag.Type}) = {tag.Description}"));

        foreach (var line in lines)
        {
            consoleWriter.WriteLine(line);
        }

        return Task.FromResult(true);
    }
}
