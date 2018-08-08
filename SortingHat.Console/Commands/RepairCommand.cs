using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    class RepairCommand : ICommand
    {
        private const string Command = "repair";

        public RepairCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }
    }
}
