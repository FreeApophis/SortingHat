using System.Collections.Generic;

namespace SortingHat.CLI.Commands
{
    interface ICommand
    {
        bool Match(IEnumerable<string> arguments);
        bool Execute(IEnumerable<string> arguments);
    }
}
