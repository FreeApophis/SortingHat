using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class CreatedAtAutoTag : DateAutoTag
    {
        public override string AutoTagKey => "CreatedAt.<>";
        public override string Description => "The date and time of the creation of the file.";

        protected override string? HandleTag(FileInfo file, IDateTagPart tagPart)
        {
            return tagPart.Select(file.CreationTime);
        }
    }
}
