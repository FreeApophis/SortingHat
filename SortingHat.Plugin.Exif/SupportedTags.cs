using MetadataExtractor.Formats.Exif;
using SortingHat.Plugin.Exif.TagTransformer;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.Exif
{
    internal class SupportedTags
    {
        public static Dictionary<string, ExifTag> GetSupportedTags()
        {
            var stringTag = new StringExifTagTransfomrer<ExifSubIfdDirectory>();
            var yearTag = new DateExifTagTransformer<ExifSubIfdDirectory, YearPart>();
            var monthTag = new DateExifTagTransformer<ExifSubIfdDirectory, MonthPart>();
            var dayTag = new DateExifTagTransformer<ExifSubIfdDirectory, DayPart>();

            var result = new List<ExifTag>()
            {
                new ExifTag() { Name = "Camera.Make", DirectoryEntryID = ExifDirectoryBase.TagMake, GetTag = stringTag },
                new ExifTag() { Name = "Camera.Model", DirectoryEntryID = ExifDirectoryBase.TagModel, GetTag = stringTag },
                new ExifTag() { Name = "Camera.Taken.Year", DirectoryEntryID = ExifDirectoryBase.TagDateTimeOriginal, GetTag = yearTag },
                new ExifTag() { Name = "Camera.Taken.Month", DirectoryEntryID = ExifDirectoryBase.TagDateTimeOriginal, GetTag = monthTag },
                new ExifTag() { Name = "Camera.Taken.Day", DirectoryEntryID = ExifDirectoryBase.TagDateTimeOriginal, GetTag = dayTag },
            };

            return result.ToDictionary(tag => tag.Name, tag => tag);
        }
    }
}
