using MetadataExtractor.Formats.Exif;
using MetadataExtractor;
using SortingHat.API.Tagging;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.Exif
{
    public abstract class ExifAutoTag : IAutoTag
    {
        public abstract IEnumerable<string> PossibleAutoTags { get; }
        public abstract string HandleTag(string fileName, string autoTag);

        protected IEnumerable<Directory> ReadMetaData(string fileName)
        {
            return ImageMetadataReader.ReadMetadata(fileName);

        }

        protected string ReadString<Type>(IEnumerable<Directory> directories)
        {
            // obtain the Exif SubIFD directory
            var directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (directory == null)
                return null;

            // query the tag's value
            return directory.GetString(ExifDirectoryBase.TagModel);
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
