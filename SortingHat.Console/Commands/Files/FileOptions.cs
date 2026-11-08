using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

/// <summary>The options that several file commands share.</summary>
internal static class FileOptions
{
    public static readonly OptionDefinition Recursive = new() { Name = "recursive", ShortName = "r", Description = "Search the subfolders too; a folder argument needs it" };

    /// <summary>Tags and files share one argument, because only the last argument can take many values. A tag starts with a colon.</summary>
    public static readonly ArgumentDefinition TagsAndFiles = new()
    {
        Name = "tags-and-files",
        Description = "Tags such as ':tax:2018' and files or file patterns such as '*.pdf', in any order.",
        Arity = Arity.OneOrMore,
    };
}
