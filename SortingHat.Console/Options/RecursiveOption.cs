using Funcky.Monads;
using SortingHat.API.DI;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Options
{
    class RecursiveOption : IOption
    {
        public Option<string> ShortOption => Option.Some("r");

        public Option<string> LongOption => Option.Some("recursive");

        public string ShortHelp => "Iterate through directories recursivly";
    }
}
