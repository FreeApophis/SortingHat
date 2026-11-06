using System.Linq;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using static MetadataExtractor.ImageMetadataReader;

namespace SortingHat.Plugin.Exif.AutoTag;

[UsedImplicitly]
public class DateExifAutoTag<TDirectory>(int directoryEntryId, string baseKey, string description) : DateAutoTag
    where TDirectory : Directory
{
    public override string AutoTagKey => $"{baseKey}.<>";
    public override string Description { get; } = description;

    protected override string? HandleTag(System.IO.FileInfo file, IDateTagPart tagPart)
    {
        var directories = ReadMetadata(file.FullName);

        // obtain the Exif SubIFD directory
        var directory = directories.OfType<TDirectory>().FirstOrDefault();

        if (directory != null && directory.TryGetDateTime(directoryEntryId, out var dateTime))
        {
            return tagPart.Select(dateTime);
        }

        return null;
    }
}
