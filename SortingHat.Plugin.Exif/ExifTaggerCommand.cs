using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.DI;
using System.Collections.Generic;
using apophis.CLI.Writer;
using Funcky.Monads;

namespace SortingHat.Plugin.Exif
{
    using static ImageMetadataReader;

    [UsedImplicitly]
    internal class ExifCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly IFilePathExtractor _filePathExtractor;

        public ExifCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor)
        {
            _consoleWriter = consoleWriter;
            _filePathExtractor = filePathExtractor;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            foreach (var filePath in _filePathExtractor.FromFilePatterns(arguments, false))
            {
                IEnumerable<Directory> directories = ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        _consoleWriter.WriteLine($"{directory.Name} - {tag.Name} ({tag.Type}) = {tag.Description}");
                    }
                }
            }
            return true;
        }

        public string LongCommand => "exif";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Reads exif information from files.";

        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
