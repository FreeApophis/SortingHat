using MetadataExtractor;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.Exif
{
    class MetaDataReader<TDirectory> : IMetaDataReader where TDirectory : Directory
    {
        protected IEnumerable<Directory> ReadMetaData(string fileName)
        {
            return ImageMetadataReader.ReadMetadata(fileName);
        }

        public string ReadString(string fileName, int directoryEntryID)
        {
            IEnumerable<Directory> directories = ReadMetaData(fileName);

            // obtain the Exif SubIFD directory
            TDirectory directory = directories.OfType<TDirectory>().FirstOrDefault();

            return directory?.GetString(directoryEntryID);
        }
    }
}
