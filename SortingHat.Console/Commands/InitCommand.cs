using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class InitCommand : ICommand
    {
        private const string Command = "init";
        private readonly IDatabase _db;

        public InitCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                _db.Setup();
            }

            return false;
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
