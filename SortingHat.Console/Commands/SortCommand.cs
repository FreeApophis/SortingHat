using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class SortCommand : ICommand
    {
        public SortCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            throw new NotImplementedException();
        }

        public string LongCommand => "sort";
        public string ShortCommand => null;

        public string ShortHelp => "";

    }
}
