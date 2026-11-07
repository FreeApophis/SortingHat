using Autofac;
using JetBrains.Annotations;
using MetadataExtractor.Formats.Exif;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using SortingHat.Plugin.Exif.AutoTag;

namespace SortingHat.Plugin.Exif;

[UsedImplicitly]
internal class ExifTaggerModule : Autofac.Module, IPlugin
{
    public string Name => "Exif Tagger";
    public string Description => "This plugin can automatically tag files according to their exif tags.";

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ExifCommand>().As<IDeclaredCommand>();

        RegisterSupportedTags(builder);
    }

    private static void RegisterSupportedTags(ContainerBuilder builder)
    {
        builder.RegisterInstance(new ConstantExifAutoTag<ExifIfd0Directory>(ExifDirectoryBase.TagMake, "Camera.Make", "The maker of the camera that took the photo.")).As<IAutoTag>();
        builder.RegisterInstance(new ConstantExifAutoTag<ExifIfd0Directory>(ExifDirectoryBase.TagModel, "Camera.Model", "The model of the camera that took the photo.")).As<IAutoTag>();
        builder.RegisterInstance(new DateExifAutoTag<ExifSubIfdDirectory>(ExifDirectoryBase.TagDateTimeOriginal, "Camera.Taken", "When the photo was taken.")).As<IAutoTag>();
    }
}
