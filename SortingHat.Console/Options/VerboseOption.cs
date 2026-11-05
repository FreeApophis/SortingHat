using Funcky.Monads;
using SortingHat.API.DI;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Options
{
    class VerboseOption : IOption
    {
        public Option<string> ShortOption => Option.Some("v");

        public Option<string> LongOption => Option.Some("verbose");

        public string ShortHelp => "More verbose output!";
    }
}
