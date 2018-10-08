using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class FolderFromRootAutoTag : IntegerAutoTag
    {
        public override string AutoTagKey => "Folder.FromRoot.<>";

        public override string Description =>
            @"Path element counted from the left: C:\User\Path 0 => C, 1 => User, 2 => Path";

        protected override string HandleTag(FileInfo file, int index)
        {
            var pathElements = PathHelper.PathElements(file.Directory);

            if (index < 0 || index >= pathElements.Count)
            {
                return null;
            }

            return pathElements[pathElements.Count - index - 1];
        }
    }
}
