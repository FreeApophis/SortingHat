using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class UpdatedAtAutoTag : DateAutoTag
    {
        public override string AutoTagKey => "UpdatedAt.<>";
        public override string Description => "The date and time of the last update of the file.";

        protected override string? HandleTag(FileInfo file, IDateTagPart tagPart)
        {
            return tagPart.Select(file.LastWriteTime);
        }
    }
}
