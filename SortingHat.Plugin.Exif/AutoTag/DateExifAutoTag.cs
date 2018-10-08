using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;
using System.Linq;

namespace SortingHat.Plugin.Exif.AutoTag
{
    using static ImageMetadataReader;

    [UsedImplicitly]
    public class DateExifAutoTag<TDirectory> : DateAutoTag where TDirectory : Directory
    {
        private readonly string _baseKey;
        private readonly int _directoryEntryID;
        public override string AutoTagKey => $"{_baseKey}.<>";
        public override string Description { get; }

        public DateExifAutoTag(int directoryEntryID, string baseKey, string description)
        {
            _directoryEntryID = directoryEntryID;
            _baseKey = baseKey;
            Description = description;
        }

        protected override string HandleTag(System.IO.FileInfo file, IDateTagPart datePart)
        {
            var directories = ReadMetadata(file.FullName);

            // obtain the Exif SubIFD directory
            var directory = directories.OfType<TDirectory>().FirstOrDefault();

            if (directory != null && directory.TryGetDateTime(_directoryEntryID, out var dateTime))
            {
                return datePart.Select(dateTime);
            }

            return null;
        }
    }
}
