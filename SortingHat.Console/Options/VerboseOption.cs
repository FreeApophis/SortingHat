using Funcky.Monads;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Options;

internal class VerboseOption : IOption
{
    public Option<string> ShortOption => Option.Some("v");

    public Option<string> LongOption => Option.Some("verbose");

    public string ShortHelp => "More verbose output!";
}
