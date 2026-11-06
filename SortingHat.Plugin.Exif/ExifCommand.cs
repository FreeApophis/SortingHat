using System.Collections.Generic;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.DI;
using static MetadataExtractor.ImageMetadataReader;

namespace SortingHat.Plugin.Exif;

[UsedImplicitly]
internal class ExifCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor) : ICommand
{
    public string LongCommand => "exif";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Reads exif information from files.";

    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var filePath in filePathExtractor.FromFilePatterns(arguments, false))
        {
            IEnumerable<Directory> directories = ReadMetadata(filePath);
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    consoleWriter.WriteLine($"{directory.Name} - {tag.Name} ({tag.Type}) = {tag.Description}");
                }
            }
        }

        return true;
    }
}
