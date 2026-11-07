using System;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class UpdatedAtAutoTag : DateAutoTag
{
    public override string Name => "UpdatedAt";

    public override string Description => "The date and time of the last update of the file.";

    protected override Option<DateTime> Date(AutoTagFile file)
        => file.Info.LastWriteTime;
}
