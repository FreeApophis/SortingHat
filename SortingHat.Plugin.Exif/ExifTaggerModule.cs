using Autofac;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System;
using System.Reflection;
using MetadataExtractor.Formats.Exif;
using SortingHat.API.AutoTag;
using SortingHat.Plugin.Exif.AutoTag;

namespace SortingHat.Plugin.Exif
{
    [UsedImplicitly]
    internal class ExifTaggerModule : Autofac.Module, IPlugin
    {
        public string Name => "Exif Tagger";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Description => "This plugin can automatically tag files according to their exif tags.";

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExifCommand>().As<ICommand>();

            RegisterSupportedTags(builder);
        }

        private static void RegisterSupportedTags(ContainerBuilder builder)
        {
            builder.Register(c => new ConstantExifAutoTag<ExifIfd0Directory>(ExifDirectoryBase.TagMake, "Camera.Make", "TODO")).As<IAutoTag>();
            builder.Register(c => new ConstantExifAutoTag<ExifIfd0Directory>(ExifDirectoryBase.TagModel, "Camera.Model", "TODO")).As<IAutoTag>();
            builder.Register(c => new DateExifAutoTag<ExifSubIfdDirectory>(ExifDirectoryBase.TagDateTimeOriginal, "Camera.Taken", "TODO")).As<IAutoTag>();
        }
    }
}
