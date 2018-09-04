using Autofac;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System.Reflection;
using System;

namespace ExifTaggerPlugin
{
    class ExifTaggerModule : Autofac.Module, IPlugin
    {
        public string Name => "Exif Tagger";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their exif tags.";

        public bool Execute()
        {
            return true;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExifTaggerCommand>().As<ICommand>();
        }
    }
}
