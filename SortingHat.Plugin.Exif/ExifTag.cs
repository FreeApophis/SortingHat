using System;

namespace SortingHat.Plugin.Exif
{
    internal class ExifTag
    {
        public string Name { get; set; }
        public int DirectoryEntryID { get; set; }
        public IMetaDataReader GetTag { get; set; }
    }
}
