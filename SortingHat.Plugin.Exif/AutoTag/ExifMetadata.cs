using System.Collections.Generic;
using System.IO;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using Directory = MetadataExtractor.Directory;

namespace SortingHat.Plugin.Exif.AutoTag;

/// <summary>The metadata of a file, read once however many EXIF variables a mask uses.</summary>
internal static class ExifMetadata
{
    public static IReadOnlyList<Directory> Of(AutoTagFile file)
        => file.Remember("Exif.Metadata", Read);

    /// <summary>A file in a format MetadataExtractor does not know, such as a text file, has no metadata.</summary>
    private static IReadOnlyList<Directory> Read(FileInfo file)
    {
        try
        {
            return [.. ImageMetadataReader.ReadMetadata(file.FullName)];
        }
        catch (ImageProcessingException)
        {
            return [];
        }
    }
}
