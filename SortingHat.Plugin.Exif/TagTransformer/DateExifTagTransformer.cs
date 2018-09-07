using MetadataExtractor;
using System.Linq;
using System;

namespace SortingHat.Plugin.Exif.TagTransformer
{
    using static ImageMetadataReader;

    internal class DateExifTagTransformer<TDirectory, TDateChooser> : ITagTransformer
        where TDirectory : Directory
        where TDateChooser : IDatePart, new()
    {
        public string TransformTag(string fileName, int directoryEntryID)
        {
            var directories = ReadMetadata(fileName);

            // obtain the Exif SubIFD directory
            var directory = directories.OfType<TDirectory>().FirstOrDefault();


            if (directory != null && directory.TryGetDateTime(directoryEntryID, out var dateTime))
            {
                var datePart = new TDateChooser();
                return datePart.Select(dateTime);
            }

            throw new NotImplementedException();
        }
    }
}
