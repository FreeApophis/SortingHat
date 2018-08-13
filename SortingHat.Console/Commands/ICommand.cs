using System.Collections.Generic;

namespace SortingHat.CLI.Commands
{
    interface ICommand
    {
        bool Execute(IEnumerable<string> arguments);

        string LongCommand { get; }

        /// <summary>
        /// Can be null
        /// </summary>
        string ShortCommand { get; }

        /// <summary>
        /// Should return a single line description
        /// </summary>
        /// <returns></returns>
        string ShortHelp { get; }
    }
}
