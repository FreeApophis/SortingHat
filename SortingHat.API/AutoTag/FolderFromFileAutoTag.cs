using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class FolderFromFileAutoTag : IAutoTag
{
    public string Name => "Folder.FromFile";

    public AutoTagParameter Parameter => AutoTagParameter.Integer;

    public string Description => @"Folder counted from the file: C:\User\Path\File.ext 0 => Path, 1 => User, 2 => C:\";

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => argument.Number.SelectMany(index => PathHelper.PathElements(file.Info.Directory).ElementAtOrNone(index));
}
