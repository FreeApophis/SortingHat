using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class FolderFromFileAutoTag : IntegerAutoTag
{
    public override string AutoTagKey => "Folder.FromFile.<>";

    public override string Description =>
        @"Folder counted from the file: C:\User\Path\File.ext 0 => Path, 1 => User, 2 => C:\";

    protected override string? HandleTag(FileInfo file, int index)
    {
        var pathElements = PathHelper.PathElements(file.Directory);

        if (index < 0 || index >= pathElements.Count)
        {
            return null;
        }

        return pathElements[index];
    }
}
