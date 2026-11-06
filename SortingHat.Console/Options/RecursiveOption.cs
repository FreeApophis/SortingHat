using Funcky.Monads;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Options;

internal class RecursiveOption : IOption
{
    public Option<string> ShortOption => Option.Some("r");

    public Option<string> LongOption => Option.Some("recursive");

    public string ShortHelp => "Iterate through directories recursivly";
}
