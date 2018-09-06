using SortingHat.Plugin.Exif.TagTransformer;

namespace SortingHat.Plugin.Exif
{
    internal class ExifTag
    {
        public string Name { get; set; }
        public int DirectoryEntryID { get; set; }
        public ITagTransformer GetTag { get; set; }
    }
}
