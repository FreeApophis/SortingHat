using MetadataExtractor;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.Exif.TagTransformer
{
    using static MetadataExtractor.ImageMetadataReader;

    class StringExifTagTransfomrer<TDirectory> : ITagTransformer where TDirectory : Directory
    {
        public string TransformTag(string fileName, int directoryEntryID)
        {
            IEnumerable<Directory> directories = ReadMetadata(fileName);

            // obtain the Exif SubIFD directory
            TDirectory directory = directories.OfType<TDirectory>().FirstOrDefault();

            return directory?.GetString(directoryEntryID);
        }
    }
}
