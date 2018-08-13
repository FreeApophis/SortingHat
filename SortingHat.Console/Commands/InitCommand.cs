using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class InitCommand : ICommand
    {
        private readonly IDatabase _db;

        public InitCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 0)
            {
                _db.Setup();
            }

            return false;
        }

        public string LongCommand => "init";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
