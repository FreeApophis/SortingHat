using SortingHat.API.DI;
using SortingHat.API;
using System.Collections.Generic;
using System;

namespace SortingHat.Plugin.Exif
{
    using MetadataExtractor;
    using static MetadataExtractor.ImageMetadataReader;

    class ExifCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            var filePaths = new FilePathExtractor(arguments);

            foreach (var filePath in filePaths.FilePaths)
            {
                IEnumerable<Directory> directories = ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
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
