using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class FolderFromRootAutoTag : IAutoTag
{
    public string Name => "Folder.FromRoot";

    public AutoTagParameter Parameter => AutoTagParameter.Integer;

    public string Description => @"The folder of the file, counted from the root: C:\User\Path\File.ext 0 => C, 1 => User, 2 => Path";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => from index in argument.Number
           from folder in Option.FromNullable(file.Info.DirectoryName)
           from name in PathHelper.Folders(folder).ElementAtOrNone(index)
           select name;
}
