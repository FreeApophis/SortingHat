using Autofac;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System.Reflection;
using System;
using SortingHat.API.Tagging;

namespace SortingHat.Plugin.Exif
{
    class ExifTaggerModule : Autofac.Module, IPlugin
    {
        public string Name => "Exif Tagger";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their exif tags.";

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExifCommand>().As<ICommand>();

            builder.RegisterType<ExifAutoTag>().As<IAutoTag>().SingleInstance();
        }
    }
}
