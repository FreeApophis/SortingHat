using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class RepairCommand : ICommand
    {
        public RepairCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }


        public string LongCommand => "repair";
        public string ShortCommand => null;

        public string ShortHelp => "";
    }
}
