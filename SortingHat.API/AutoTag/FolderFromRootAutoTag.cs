using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class FolderFromRootAutoTag : IAutoTag
{
    public string Name => "Folder.FromRoot";

    public AutoTagParameter Parameter => AutoTagParameter.Integer;

    public string Description => @"Path element counted from the left: C:\User\Path 0 => C, 1 => User, 2 => Path";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => from index in argument.Number
           let elements = PathHelper.PathElements(file.Info.Directory)
           from element in elements.ElementAtOrNone(elements.Count - index - 1)
           select element;
}
