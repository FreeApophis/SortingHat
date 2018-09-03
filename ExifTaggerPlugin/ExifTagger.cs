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
            return true;
        }

    }
}
