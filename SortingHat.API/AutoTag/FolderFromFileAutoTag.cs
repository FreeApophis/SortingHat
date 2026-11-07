using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class FolderFromFileAutoTag : IAutoTag
{
    public string Name => "Folder.FromFile";

    public AutoTagParameter Parameter => AutoTagParameter.Integer;

    public string Description => @"The folder of the file, counted from the file: C:\User\Path\File.ext 0 => Path, 1 => User, 2 => C";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => from index in argument.Number
           from folder in Option.FromNullable(file.Info.DirectoryName)
           from name in PathHelper.Folders(folder).Reverse().ElementAtOrNone(index)
           select name;
}
