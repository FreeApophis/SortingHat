using System.Linq;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using static MetadataExtractor.ImageMetadataReader;

namespace SortingHat.Plugin.Exif.AutoTag;

[UsedImplicitly]
public class DateExifAutoTag<TDirectory> : DateAutoTag
    where TDirectory : Directory
{
    private readonly string _baseKey;
    private readonly int _directoryEntryId;

    public DateExifAutoTag(int directoryEntryId, string baseKey, string description)
    {
        _directoryEntryId = directoryEntryId;
        _baseKey = baseKey;
        Description = description;
    }

    public override string AutoTagKey => $"{_baseKey}.<>";
    public override string Description { get; }

    protected override string? HandleTag(System.IO.FileInfo file, IDateTagPart tagPart)
    {
        var directories = ReadMetadata(file.FullName);

        // obtain the Exif SubIFD directory
        var directory = directories.OfType<TDirectory>().FirstOrDefault();

        if (directory != null && directory.TryGetDateTime(_directoryEntryId, out var dateTime))
        {
            return tagPart.Select(dateTime);
        }

        return null;
    }
}
