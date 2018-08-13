using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class FileMoveCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public string LongCommand => "move-files";
        public string ShortCommand => null;
        public string ShortHelp => "";

    }
}
