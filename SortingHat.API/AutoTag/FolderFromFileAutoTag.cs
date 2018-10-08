using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class FolderFromFileAutoTag : IntegerAutoTag
    {
        public override string AutoTagKey => "Folder.FromFile.<>";

        public override string Description =>
            @"Path element counted from the right: C:\User\File.ext 0 => File.ext, 1 => User, 2 => C:";

        protected override string HandleTag(FileInfo file, int index)
        {
            var pathElements = PathHelper.PathElements(file.Directory);

            if (index < 0 || index >= pathElements.Count) {
                return null;
            }

            return pathElements[index];
        }
    }
}
