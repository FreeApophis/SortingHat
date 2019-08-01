using Funcky.Monads;
using SortingHat.API.DI;


namespace SortingHat.CLI.Options
{
    internal class DryRunOption : IOption
    {
        public Option<string> ShortOption => Option<string>.None();

        public Option<string> LongOption => Option.Some("dry-run");

        public string ShortHelp => "Dry run. No changes are written to the database.";
    }
}
