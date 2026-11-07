using System;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class CreatedAtAutoTag : DateAutoTag
{
    public override string Name => "CreatedAt";

    public override string Description => "The date and time of the creation of the file.";

    protected override Option<DateTime> Date(AutoTagFile file)
        => file.Info.CreationTime;
}
