using System;
using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.Desktop.Shell;

/// <summary>The desktop app cannot ask yet; like hat without a terminal, every question is left unanswered and the caller decides.</summary>
[UsedImplicitly]
internal sealed class NoUserPrompt : IUserPrompt
{
    public Option<bool> Confirm(string question)
        => Option<bool>.None;

    public Option<T> Choose<T>(string question, IReadOnlyList<T> choices, Func<T, string> describe)
        where T : notnull
        => Option<T>.None;
}
