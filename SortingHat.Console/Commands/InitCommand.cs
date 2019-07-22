using JetBrains.Annotations;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class InitCommand : ICommand
    {
        private readonly IDatabase _db;

        public InitCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            if (arguments.Any() == false)
            {
                _db.Setup();
            }

            return false;
        }

        public string LongCommand => "init";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Initializes the database, a new database is created";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
