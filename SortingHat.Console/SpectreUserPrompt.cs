using System;
using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>
/// Asks with Spectre.Console's prompts, for hat's commands and for plugins alike, but only in an interactive terminal.
/// Questions and choices are shown as text, never as markup, so a '[' in a path stays.
/// </summary>
[UsedImplicitly]
internal sealed class SpectreUserPrompt(IAnsiConsole console) : IUserPrompt
{
    private bool CanAsk
        => console.Profile.Capabilities.Interactive;

    public Option<bool> Confirm(string question)
        => CanAsk
            ? Option.Some(console.Prompt(new ConfirmationPrompt(Markup.Escape(question)) { DefaultValue = false }))
            : Option<bool>.None;

    public Option<T> Choose<T>(string question, IReadOnlyList<T> choices, Func<T, string> describe)
        where T : notnull
        => choices switch
        {
            [] => throw new ArgumentException("There is nothing to choose from.", nameof(choices)),
            _ when CanAsk => Option.Some(console.Prompt(new SelectionPrompt<T>()
                .Title(Markup.Escape(question))
                .UseConverter(choice => Markup.Escape(describe(choice)))
                .AddChoices(choices))),
            _ => Option<T>.None,
        };
}
