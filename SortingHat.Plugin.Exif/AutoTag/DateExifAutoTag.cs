using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using Directory = MetadataExtractor.Directory;

namespace SortingHat.Plugin.Exif.AutoTag;

/// <summary>A date entry of the metadata, such as when the photo was taken.</summary>
[UsedImplicitly]
public class DateExifAutoTag<TDirectory>(int directoryEntryId, string name, string description) : DateAutoTag
    where TDirectory : Directory
{
    public override string Name => name;

    public override string Description => description;

    protected override Option<DateTime> Date(AutoTagFile file)
        => ExifMetadata.Of(file)
            .OfType<TDirectory>()
            .FirstOrNone()
            .SelectMany(directory => directory.TryGetDateTime(directoryEntryId, out var date) ? Option.Some(date) : Option<DateTime>.None);
}
