using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using SortingHat.API.Tagging;
using static SortingHat.Plugin.Exif.SupportedTags;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.Plugin.Exif
{
    public class ExifAutoTag : IAutoTag
    {
        private Dictionary<string, ExifTag> _supportedTags = GetSupportedTags();
        public IEnumerable<string> PossibleAutoTags => _supportedTags.Select(t => t.Key);

        public string HandleTag(string fileName, string autoTag)
        {
            if (_supportedTags.TryGetValue(autoTag, out var exifTag))
            {
                exifTag.GetTag.ReadString(fileName, exifTag.DirectoryEntryID);
            }

            throw new NotImplementedException();
        }

        //DateTime? GetTakenDateTime(IEnumerable<Directory> directories)
        //{
        //    // obtain the Exif SubIFD directory
        //    var directory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        //    if (directory == null)
        //        return null;

        //    // query the tag's value
        //    if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime))
        //        return dateTime;

        //    return null;
        //}


    }
}
