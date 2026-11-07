using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using Directory = MetadataExtractor.Directory;

namespace SortingHat.Plugin.Exif.AutoTag;

/// <summary>A text entry of the metadata, such as the camera make.</summary>
[UsedImplicitly]
public class ConstantExifAutoTag<TDirectory>(int directoryEntryId, string name, string description) : IAutoTag
    where TDirectory : Directory
{
    public string Name => name;

    public AutoTagParameter Parameter => AutoTagParameter.None;

    public string Description => description;

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => ExifMetadata.Of(file)
            .OfType<TDirectory>()
            .FirstOrNone()
            .SelectMany(directory => Option.FromNullable(directory.GetString(directoryEntryId)));
}
