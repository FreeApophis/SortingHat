using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using SortingHat.API.AutoTag;

namespace SortingHat.Plugin.Exif
{
    using static SupportedTags;

    [UsedImplicitly]
    public class ExifAutoTag : IAutoTag
    {
        private readonly Dictionary<string, ExifTag> _supportedTags = GetSupportedTags();
        public IEnumerable<string> PossibleAutoTags => _supportedTags.Select(t => t.Key);

        public string HandleTag(string autoTag, string fileName)
        {
            return _supportedTags.TryGetValue(autoTag, out var exifTag)
                ? exifTag.GetTag.TransformTag(fileName, exifTag.DirectoryEntryID)
                : null;
        }
    }
}
