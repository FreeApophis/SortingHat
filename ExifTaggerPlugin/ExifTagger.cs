using MetadataExtractor;
using SortingHat.API.Plugin;
using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace ExifTaggerPlugin
{
    [UsedImplicitly]
    public class ExifTagger : IPlugin
    {
        public string Name => "Exif Tagger";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their exif tags.";

        private readonly ICommand _exifTaggerCommand = new ExifTaggerCommand();

        public void Register(List<ICommand> pluginCommands)
        {
            pluginCommands.Add(_exifTaggerCommand);
        }

        public bool Execute()
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata("C:\\Users\\Thoma\\Pictures\\5D MKII\\GM1B3543.jpg");
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                }
            }

            return true;
        }

    }
}
