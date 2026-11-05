using Funcky.Monads;
using SortingHat.API.DI;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI.Options
{
    internal class OpenOption : IOption
    {
        public Option<string> ShortOption => Option<string>.None;

        public Option<string> LongOption => Option.Some("open");

        public string ShortHelp => "Opens the file with the associated program.";
    }
}