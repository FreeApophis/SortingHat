using MetadataExtractor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.Plugin.Exif.TagTransformer
{
    using static MetadataExtractor.ImageMetadataReader;


    class DateExifTagTransformer<TDirectory, TDateChooser> : ITagTransformer
        where TDirectory : Directory
        where TDateChooser : IDatePart, new()
    {

        string ChoseDatePart(DateTime dateTime)
        {
            return dateTime.Year.ToString();
        }

        public string TransformTag(string fileName, int directoryEntryID)
        {
            IEnumerable<Directory> directories = ReadMetadata(fileName);

            // obtain the Exif SubIFD directory
            TDirectory directory = directories.OfType<TDirectory>().FirstOrDefault();


            if (directory != null && directory.TryGetDateTime(directoryEntryID, out var dateTime))
            {
                var datePart = new TDateChooser();
                return datePart.Select(dateTime);
            }

            throw new NotImplementedException();
        }
    }
}
