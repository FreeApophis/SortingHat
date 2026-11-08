using System;
using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.API.DI;

/// <summary>
/// Asks the user in the terminal, the way hat's own commands do. hat can only ask in an interactive terminal:
/// in a script, every question returns none, so a command must decide what happens without an answer,
/// such as skipping with a reason or offering an option like <c>--yes</c>.
/// </summary>
public interface IUserPrompt
{
    /// <summary>A yes/no question: only 'y' is yes, Enter or 'n' is no, anything else asks again. None when hat cannot ask.</summary>
    Option<bool> Confirm(string question);

    /// <summary>One of the <paramref name="choices"/>, picked with the arrow keys and Enter, each shown as <paramref name="describe"/> says. None when hat cannot ask.</summary>
    /// <exception cref="ArgumentException">There are no choices.</exception>
    Option<T> Choose<T>(string question, IReadOnlyList<T> choices, Func<T, string> describe)
        where T : notnull;
}
