using MetadataExtractor;
using SortingHat.API.Plugin;
using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Autofac;

namespace ExifTaggerPlugin
{
    [UsedImplicitly]
    public class ExifTagger : IPlugin
    {
        public string Name => "Exif Tagger";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their exif tags.";

        public void Register(IComponentContext container, List<ICommand> pluginCommands)
        {
            pluginCommands.Add(new ExifTaggerCommand(container));
        }

        public bool Execute()
        {
            return true;
        }

    }
}
