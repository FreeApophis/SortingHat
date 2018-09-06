using MetadataExtractor.Formats.Exif;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.Exif
{
    internal class SupportedTags
    {
        public static Dictionary<string, ExifTag> GetSupportedTags()
        {
            var exifDirectory = new MetaDataReader<ExifSubIfdDirectory>();

            var result = new List<ExifTag>()
            {
                new ExifTag() {
                    Name = "Camera.Make",
                    DirectoryEntryID = ExifDirectoryBase.TagModel,
                    GetTag = exifDirectory
        }
    };

            return result.ToDictionary(tag => tag.Name, tag => tag);
        }
    }
}
