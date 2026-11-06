using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;

namespace SortingHat.Plugin.Exif.AutoTag;

[UsedImplicitly]
public class ConstantExifAutoTag<TDirectory>(int directoryEntryId, string baseKey, string description) : ConstantAutoTag
    where TDirectory : Directory
{
    public override string AutoTagKey => $"{baseKey}";
    public override string Description { get; } = description;

    protected override string? HandleTag(System.IO.FileInfo file)
    {
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(file.FullName);

        // obtain the Exif SubIFD directory
        var directory = directories.OfType<TDirectory>().FirstOrDefault();

        return directory?.GetString(directoryEntryId);
    }
}
