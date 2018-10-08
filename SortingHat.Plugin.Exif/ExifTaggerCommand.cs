using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;

namespace SortingHat.Plugin.Exif
{
    using static ImageMetadataReader;

    [UsedImplicitly]
    internal class ExifCommand : ICommand
    {
        private readonly IFilePathExtractor _filePathExtractor;

        public ExifCommand(IFilePathExtractor filePathExtractor)
        {
            _filePathExtractor = filePathExtractor;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var filePath in _filePathExtractor.FromFilePatterns(arguments))
            {
                IEnumerable<Directory> directories = ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        Console.WriteLine($"{directory.Name} - {tag.Name} ({tag.Type}) = {tag.Description}");
                    }
                }
            }
            return true;
        }

        public string LongCommand => "exif";
        public string ShortCommand => null;
        public string ShortHelp => "Reads exif information from files.";

        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
